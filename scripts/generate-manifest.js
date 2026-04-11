#!/usr/bin/env node
/**
 * generate-manifest.js
 * Scans CSharpStubs/*.cs and emits Playground/wwwroot/api-manifest.json.
 * Run: node scripts/generate-manifest.js
 *
 * Output shape:
 * {
 *   types:         { McPlayer: { doc, javaClass, members: [...] }, ... }
 *   staticClasses: { Events: { members: [...] }, ... }
 *   interfaces:    { IMod: { members: [...] }, ... }
 *   events:        { PlayerJoin: { doc, fabricClass, fabricEvent, paramTypes: [...] }, ... }
 * }
 *
 * Member shape:
 *   { name, kind: 'property'|'method'|'event', type?, returnType?, params?, doc, javaTemplate? }
 */

'use strict';

const fs   = require('fs');
const path = require('path');

const stubsDir   = path.join(__dirname, '../CSharpStubs');
const outputFile = path.join(__dirname, '../Playground/wwwroot/api-manifest.json');

// ── helpers ───────────────────────────────────────────────────────────────────

/** Strip leading `/// ` prefixes from a block of doc-comment lines. */
function cleanDoc(raw) {
    return raw
        .split('\n')
        .map(l => l.replace(/^\s*\/\/\/\s?/, '').trim())
        .join(' ')
        .replace(/\s+/g, ' ')
        .trim();
}

/**
 * Extract the XML <summary> immediately preceding a named member or class.
 * `anchor` is the regex fragment that must follow the closing </summary>.
 */
function extractSummary(content, anchorRe) {
    // Match: /// <summary> ... /// </summary> ... <anchor>
    const re = new RegExp(
        /(?:\/\/\/[^\n]*\n)*\/\/\/\s*<summary>([\s\S]*?)\/\/\/\s*<\/summary>/.source +
        /[\s\S]{0,300}?/.source +
        anchorRe,
        'm'
    );
    const m = content.match(re);
    return m ? cleanDoc(m[1]) : '';
}

/** Pull generic type arguments out of Action<A, B, C>. */
function parseActionArgs(actionType) {
    const m = actionType.match(/^Action<(.+)>$/);
    if (!m) return [];
    // top-level comma split
    const args = [];
    let depth = 0, start = 0;
    for (let i = 0; i < m[1].length; i++) {
        const c = m[1][i];
        if (c === '<') depth++;
        else if (c === '>') depth--;
        else if (c === ',' && depth === 0) {
            args.push(m[1].slice(start, i).trim());
            start = i + 1;
        }
    }
    args.push(m[1].slice(start).trim());
    return args;
}

// ── per-file parsers ──────────────────────────────────────────────────────────

/**
 * Extract all members (properties, methods) from class/interface body.
 * Looks for [JavaMethod("...")] attribute lines + following declaration.
 */
function extractMembers(content) {
    const members = [];
    const seen    = new Set();

    // --- Properties (with or without [JavaMethod]) ---
    // Pattern: optional [JavaMethod("...")], then public <type> <Name> { get; ... }
    const propRe = /(?:\[JavaMethod\("([^"]+)"\)\]\s*)?(?:\/\/\/[^\n]*\n\s*)*public\s+(?:(?:static|override|virtual|new)\s+)*([\w<>?[\], ]+?)\s+(\w+)\s*\{\s*(?:get|set)/g;
    let m;
    while ((m = propRe.exec(content)) !== null) {
        const name = m[3];
        if (seen.has('prop:' + name)) continue;
        seen.add('prop:' + name);

        // grab the doc comment block just before this match
        const before = content.slice(Math.max(0, m.index - 400), m.index);
        const docM   = before.match(/\/\/\/\s*<summary>([\s\S]*?)\/\/\/\s*<\/summary>\s*$/);
        const doc    = docM ? cleanDoc(docM[1]) : '';

        members.push({
            name,
            kind:         'property',
            type:         m[2].trim(),
            doc,
            javaTemplate: m[1] || null,
        });
    }

    // --- Methods (with or without [JavaMethod]) ---
    // Pattern: optional [JavaMethod("...")], public <ret> <Name>(<params>)
    const methodRe = /(?:\[JavaMethod\("([^"]+)"\)\]\s*)?(?:\/\/\/[^\n]*\n\s*)*public\s+(?:(?:static|override|virtual|new)\s+)*([\w<>?[\], ]+?)\s+(\w+)\s*\(([^)]*)\)/g;
    while ((m = methodRe.exec(content)) !== null) {
        const name = m[3];
        // skip property-like (already captured)
        if (seen.has('prop:' + name)) continue;
        // skip constructors (name matches class), operators, common junk
        if (/^(get|set|add|remove)$/.test(name)) continue;
        if (seen.has('meth:' + name + ':' + m[4])) continue;
        seen.add('meth:' + name + ':' + m[4]);

        const before = content.slice(Math.max(0, m.index - 400), m.index);
        const docM   = before.match(/\/\/\/\s*<summary>([\s\S]*?)\/\/\/\s*<\/summary>\s*$/);
        const doc    = docM ? cleanDoc(docM[1]) : '';

        const rawParams = m[4].trim();
        const params = rawParams === '' ? [] : rawParams.split(',').map(p => {
            p = p.trim();
            const parts = p.split(/\s+/);
            return {
                type: parts.slice(0, -1).join(' ') || 'object',
                name: parts[parts.length - 1].replace(/^@/, ''),
            };
        });

        members.push({
            name,
            kind:         'method',
            returnType:   m[2].trim(),
            params,
            doc,
            javaTemplate: m[1] || null,
        });
    }

    return members;
}

/**
 * Extract events from a static Events class.
 * Returns { memberEvents (for staticClasses.Events.members), topEvents (for manifest.events) }
 */
function extractEvents(content) {
    const memberEvents = [];
    const topEvents    = {};

    // [JavaEvent("FabricClass", "EVENT")]
    // public static event Action<T1, T2> EventName = null!;
    const eventRe = /\[JavaEvent\("(\w+)",\s*"(\w+)"\)\]\s*(?:\/\/\/[^\n]*\n\s*)*public\s+static\s+event\s+(Action(?:<[^>]+>)?)\s+(\w+)/g;
    let m;
    while ((m = eventRe.exec(content)) !== null) {
        const fabricClass = m[1];
        const fabricEvent = m[2];
        const actionType  = m[3];
        const name        = m[4];

        const before = content.slice(Math.max(0, m.index - 400), m.index);
        const docM   = before.match(/\/\/\/\s*<summary>([\s\S]*?)\/\/\/\s*<\/summary>\s*$/);
        const doc    = docM ? cleanDoc(docM[1]) : '';

        const paramTypes = parseActionArgs(actionType);

        memberEvents.push({
            name,
            kind:        'event',
            actionType,
            paramTypes,
            doc,
            fabricClass,
            fabricEvent,
        });

        topEvents[name] = { doc, fabricClass, fabricEvent, paramTypes };
    }

    return { memberEvents, topEvents };
}

// ── main parse loop ───────────────────────────────────────────────────────────

const manifest = {
    types:         {},
    staticClasses: {},
    interfaces:    {},
    events:        {},
};

const files = fs.readdirSync(stubsDir).filter(f => f.endsWith('.cs'));

for (const file of files) {
    const content = fs.readFileSync(path.join(stubsDir, file), 'utf8');

    // Detect kind
    const classM     = content.match(/(?:public\s+)?(?:static\s+)?(?:abstract\s+)?class\s+(\w+)/);
    const ifaceM     = content.match(/public\s+interface\s+(\w+)/);
    const isStatic   = /\bstatic\s+class\b/.test(content);
    const isAbstract = /\babstract\s+class\b/.test(content);

    if (ifaceM) {
        const name    = ifaceM[1];
        const members = extractMembers(content);
        manifest.interfaces[name] = { members };
    }

    if (classM) {
        const name = classM[1];

        // JavaClass attribute gives the FQN
        const javaClassM = content.match(/\[JavaClass\("([^"]+)"\)\]/);
        const javaClass  = javaClassM ? javaClassM[1] : null;

        const docM = content.match(/\/\/\/\s*<summary>([\s\S]*?)\/\/\/\s*<\/summary>[\s\S]{0,200}?class\s+\w+/);
        const doc  = docM ? cleanDoc(docM[1]) : '';

        if (name === 'Events' || isStatic) {
            const members = extractMembers(content);

            if (name === 'Events') {
                const { memberEvents, topEvents } = extractEvents(content);
                Object.assign(manifest.events, topEvents);
                manifest.staticClasses[name] = { members: [...members, ...memberEvents] };
            } else {
                manifest.staticClasses[name] = { members };
            }
        } else if (!isAbstract) {
            const members = extractMembers(content);
            manifest.types[name] = { doc, javaClass, members };
        } else {
            // abstract base classes (CustomItem, CustomBlock, etc.)
            const members = extractMembers(content);
            manifest.types[name] = { doc, javaClass, members, isAbstract: true };
        }
    }
}

// ── write output ──────────────────────────────────────────────────────────────

fs.mkdirSync(path.dirname(outputFile), { recursive: true });
fs.writeFileSync(outputFile, JSON.stringify(manifest, null, 2));

const typeCount  = Object.keys(manifest.types).length;
const staticCount = Object.keys(manifest.staticClasses).length;
const eventCount = Object.keys(manifest.events).length;
console.log(`Manifest written to ${outputFile}`);
console.log(`  types: ${typeCount}, staticClasses: ${staticCount}, events: ${eventCount}`);

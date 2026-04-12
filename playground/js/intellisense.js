/**
 * intellisense.js — Monaco intellisense provider for CSCraft C# stubs.
 *
 * Features:
 *  1. Variable type tracking  — scans the document for `McFoo bar = …` declarations
 *  2. Lambda param inference  — `Events.PlayerJoin += (p) => {` → p is McPlayer
 *  3. Dot completions         — when `bar.` is typed, shows McFoo members
 *  4. Static-class completions — `Events.`, `McServer.`, etc.
 *  5. Top-level type names    — class/method signatures auto-suggest Mc* types
 *  6. Hover provider          — shows Java template + doc for any known member
 *  7. Signature help          — shows parameter list while typing method calls
 */
'use strict';

// Called from playground.js after Monaco is ready.
async function initIntellisense(monaco, csEditor) {
    let api = null;

    try {
        const res = await fetch('api-manifest.json');
        if (!res.ok) throw new Error('HTTP ' + res.status);
        api = await res.json();
    } catch (e) {
        console.warn('[CSCraft intellisense] Could not load api-manifest.json:', e);
        return;
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    /**
     * Scan the entire document text for variable declarations.
     * Returns a Map<varName, typeName>.
     *   - Explicit: `McPlayer player =`   → player → McPlayer
     *   - Var:      `var player = someCall()` tries to infer from RHS (best-effort)
     *   - Param:    method/lambda params  `McPlayer player`
     */
    function buildVarTypeMap(code) {
        const map = new Map();

        // Explicit typed declarations:  McFoo varName =  /  McFoo varName,  / McFoo varName)
        const declRe = /\b(Mc\w+|IMod)\s+(\w+)\s*[=,)]/g;
        let m;
        while ((m = declRe.exec(code)) !== null) {
            map.set(m[2], m[1]);
        }

        // Lambda params inferred from event type:
        // Events.PlayerJoin += (player) =>   →  look up PlayerJoin in api.events
        const lambdaRe = /Events\.(\w+)\s*\+=\s*\(([^)]+)\)\s*=>/g;
        while ((m = lambdaRe.exec(code)) !== null) {
            const eventName = m[1];
            const paramList = m[2].split(',').map(p => p.trim()).filter(Boolean);
            const ev        = api.events?.[eventName];
            if (ev?.paramTypes) {
                ev.paramTypes.forEach((type, i) => {
                    if (paramList[i]) map.set(paramList[i], type);
                });
            }
        }

        return map;
    }

    /**
     * Given the text of a line up to the cursor, return the variable name before
     * the final dot (e.g. "  player.Send" → "player").
     */
    function getReceiverName(lineUpToCursor) {
        const m = lineUpToCursor.match(/(\w+)\.\w*$/);
        return m ? m[1] : null;
    }

    /**
     * Return the type name for a receiver: first check the var map, then check if
     * the receiver itself is a known type name (static class / type name used directly).
     */
    function resolveReceiverType(receiver, varMap) {
        if (varMap.has(receiver)) return varMap.get(receiver);
        // Is the receiver itself a known type or static class name?
        if (api.types?.[receiver])         return receiver;
        if (api.staticClasses?.[receiver]) return receiver;
        return null;
    }

    /** Map members to Monaco CompletionItems. */
    function membersToSuggestions(members, range, filterKind) {
        return (members || [])
            .filter(m => !filterKind || m.kind === filterKind)
            .map(m => {
                const isMethod = m.kind === 'method';
                const isEvent  = m.kind === 'event';
                const label    = m.name;
                let   insert   = m.name;

                if (isMethod && m.params?.length > 0) {
                    const snippetParams = m.params.map((p, i) => `\${${i + 1}:${p.name}}`).join(', ');
                    insert = `${m.name}(${snippetParams})`;
                } else if (isMethod) {
                    insert = `${m.name}()`;
                } else if (isEvent) {
                    insert = `${m.name} += (\${1:args}) =>\n{\n\t\${2}\n}`;
                }

                const detail = isMethod
                    ? `${m.returnType ?? 'void'} ${m.name}(${(m.params || []).map(p => p.type + ' ' + p.name).join(', ')})`
                    : (isEvent ? `event ${m.actionType ?? ''}` : `${m.type ?? ''} ${m.name}`);

                const docParts = [];
                if (m.doc)          docParts.push(m.doc);
                if (m.javaTemplate) docParts.push(`Java: \`${m.javaTemplate}\``);
                if (isEvent && m.fabricClass) docParts.push(`Fabric: ${m.fabricClass}.${m.fabricEvent}`);

                return {
                    label,
                    kind:   isMethod ? monaco.languages.CompletionItemKind.Method
                          : isEvent  ? monaco.languages.CompletionItemKind.Event
                                     : monaco.languages.CompletionItemKind.Property,
                    insertText:      insert,
                    insertTextRules: (isMethod && m.params?.length > 0) || isEvent
                        ? monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet
                        : undefined,
                    detail,
                    documentation: docParts.length ? { value: docParts.join('\n\n') } : undefined,
                    range,
                };
            });
    }

    // ── Completion provider ───────────────────────────────────────────────────

    monaco.languages.registerCompletionItemProvider('csharp', {
        triggerCharacters: ['.'],

        provideCompletionItems(model, position) {
            const wordInfo  = model.getWordUntilPosition(position);
            const range     = {
                startLineNumber: position.lineNumber,
                endLineNumber:   position.lineNumber,
                startColumn:     wordInfo.startColumn,
                endColumn:       position.column,
            };

            const lineUpTo = model.getValueInRange({
                startLineNumber: position.lineNumber, startColumn: 1,
                endLineNumber:   position.lineNumber, endColumn:   position.column,
            });

            const fullText = model.getValue();
            const varMap   = buildVarTypeMap(fullText);

            // ── 1. Dot-triggered: resolve receiver ────────────────────────────
            if (lineUpTo.endsWith('.') || lineUpTo.match(/\.\w*$/)) {
                const receiver   = getReceiverName(lineUpTo);
                const typeName   = receiver ? resolveReceiverType(receiver, varMap) : null;

                // Static class members
                if (typeName && api.staticClasses?.[typeName]) {
                    return { suggestions: membersToSuggestions(api.staticClasses[typeName].members, range) };
                }

                // Instance type members
                if (typeName && api.types?.[typeName]) {
                    return { suggestions: membersToSuggestions(api.types[typeName].members, range) };
                }

                return { suggestions: [] };
            }

            // ── 2. No dot: top-level completions ──────────────────────────────

            const suggestions = [];

            // Type names (Mc*)
            Object.entries(api.types || {}).forEach(([name, t]) => {
                suggestions.push({
                    label:  name,
                    kind:   monaco.languages.CompletionItemKind.Class,
                    detail: t.javaClass ?? '',
                    documentation: t.doc ? { value: t.doc } : undefined,
                    insertText: name,
                    range,
                });
            });

            // Static class names
            Object.keys(api.staticClasses || {}).forEach(name => {
                suggestions.push({
                    label:  name,
                    kind:   monaco.languages.CompletionItemKind.Module,
                    insertText: name,
                    range,
                });
            });

            // Interface names
            Object.keys(api.interfaces || {}).forEach(name => {
                suggestions.push({
                    label:  name,
                    kind:   monaco.languages.CompletionItemKind.Interface,
                    insertText: name,
                    range,
                });
            });

            // Common keywords / snippets
            const snippets = [
                { label: 'ModInfo',       insert: '[ModInfo("${1:modid}", "${2:Mod Name}", "${3:1.0.0}")]',  detail: 'Mod metadata attribute' },
                { label: 'IMod',          insert: 'IMod',                                                    detail: 'Mod entry-point interface' },
                { label: 'OnInitialize',  insert: 'public void OnInitialize()\n{\n\t${1}\n}',               detail: 'Mod entry-point method' },
            ];
            snippets.forEach(s => suggestions.push({
                label:           s.label,
                kind:            monaco.languages.CompletionItemKind.Snippet,
                insertText:      s.insert,
                insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                detail:          s.detail,
                range,
            }));

            return { suggestions };
        },
    });

    // ── Hover provider ────────────────────────────────────────────────────────

    monaco.languages.registerHoverProvider('csharp', {
        provideHover(model, position) {
            const word = model.getWordAtPosition(position);
            if (!word) return null;

            const lineUpTo = model.getValueInRange({
                startLineNumber: position.lineNumber, startColumn: 1,
                endLineNumber:   position.lineNumber, endColumn:   word.endColumn,
            });

            const fullText = model.getValue();
            const varMap   = buildVarTypeMap(fullText);
            const receiver = getReceiverName(lineUpTo);
            const typeName = receiver ? resolveReceiverType(receiver, varMap) : null;

            if (typeName) {
                const source  = api.types?.[typeName] ?? api.staticClasses?.[typeName];
                const member  = source?.members?.find(m => m.name === word.word);
                if (member) {
                    const lines = [];
                    if (member.doc)          lines.push(member.doc);
                    if (member.javaTemplate) lines.push(`\n**Java:** \`${member.javaTemplate}\``);
                    if (member.kind === 'event' && member.fabricClass)
                        lines.push(`\n**Fabric:** \`${member.fabricClass}.${member.fabricEvent}\``);

                    return {
                        range: new monaco.Range(position.lineNumber, word.startColumn, position.lineNumber, word.endColumn),
                        contents: [{ value: lines.join('\n') }],
                    };
                }
            }

            // Hover on a type name itself
            const typeInfo = api.types?.[word.word];
            if (typeInfo?.doc) {
                return {
                    range: new monaco.Range(position.lineNumber, word.startColumn, position.lineNumber, word.endColumn),
                    contents: [{ value: `**${word.word}**\n\n${typeInfo.doc}` + (typeInfo.javaClass ? `\n\nJava: \`${typeInfo.javaClass}\`` : '') }],
                };
            }

            return null;
        },
    });

    // ── Signature help provider ───────────────────────────────────────────────

    monaco.languages.registerSignatureHelpProvider('csharp', {
        signatureHelpTriggerCharacters: ['(', ','],

        provideSignatureHelp(model, position) {
            const lineUpTo = model.getValueInRange({
                startLineNumber: position.lineNumber, startColumn: 1,
                endLineNumber:   position.lineNumber, endColumn:   position.column,
            });

            // Find `SomeName.MethodName(` pattern
            const callM = lineUpTo.match(/(\w+)\.(\w+)\(([^)]*)$/);
            if (!callM) return null;

            const [, receiver, methodName, argsSoFar] = callM;
            const fullText = model.getValue();
            const varMap   = buildVarTypeMap(fullText);
            const typeName = resolveReceiverType(receiver, varMap);
            if (!typeName) return null;

            const source = api.types?.[typeName] ?? api.staticClasses?.[typeName];
            const method = source?.members?.find(m => m.name === methodName && m.kind === 'method');
            if (!method) return null;

            const paramCount  = (argsSoFar.match(/,/g) || []).length;
            const paramLabels = (method.params || []).map(p => `${p.type} ${p.name}`);

            return {
                value: {
                    signatures: [{
                        label:      `${method.returnType ?? 'void'} ${method.name}(${paramLabels.join(', ')})`,
                        documentation: method.doc ? { value: method.doc } : undefined,
                        parameters: paramLabels.map(lbl => ({ label: lbl })),
                    }],
                    activeSignature: 0,
                    activeParameter: Math.min(paramCount, paramLabels.length - 1),
                },
                dispose() {},
            };
        },
    });

    console.log('[CSCraft intellisense] Registered providers. Types:', Object.keys(api.types || {}).length);
}

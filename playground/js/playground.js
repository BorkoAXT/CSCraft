/**
 * playground.js — CSCraft Playground
 * Multi-file editor, Blazor WASM transpiler, download Fabric project zip,
 * localStorage persistence, shareable URLs, MC version selection.
 */
'use strict';

// ── Minecraft version configs ─────────────────────────────────────────────────
const MC_VERSIONS = {
    '1.21.4': { yarn: '1.21.4+build.8',  loader: '0.16.9',  loom: '1.9.2',  fabric_api: '0.119.5+1.21.4', java: '21' },
    '1.21.3': { yarn: '1.21.3+build.2',  loader: '0.16.7',  loom: '1.9.2',  fabric_api: '0.115.1+1.21.3', java: '21' },
    '1.21.1': { yarn: '1.21.1+build.3',  loader: '0.16.5',  loom: '1.9.2',  fabric_api: '0.116.9+1.21.1', java: '21' },
    '1.20.4': { yarn: '1.20.4+build.3',  loader: '0.15.11', loom: '1.7.4',  fabric_api: '0.97.2+1.20.4',  java: '17' },
};

// ── Built-in examples ─────────────────────────────────────────────────────────
const EXAMPLES = {
    hello: [{
        name: 'HelloMod.cs',
        content: `using CSCraft;

[ModInfo("hellomod", "Hello Mod", "1.0.0")]
public class HelloMod : IMod
{
    public void OnInitialize()
    {
        Events.PlayerJoin += (player) =>
        {
            player.SendMessage("Welcome, " + player.Name + "!");
        };
    }
}`
    }],

    item: [{
        name: 'GemMod.cs',
        content: `using CSCraft;

[ModInfo("gemmod", "Gem Mod", "1.0.0")]
public class GemMod : IMod
{
    public void OnInitialize()
    {
        McRegistry.RegisterItem("magic_gem", new McItem());

        Events.PlayerJoin += (player) =>
        {
            player.GiveItem("gemmod:magic_gem", 1);
        };
    }
}`
    }],

    block: [{
        name: 'BlockMod.cs',
        content: `using CSCraft;

[ModInfo("blockmod", "Block Mod", "1.0.0")]
public class BlockMod : IMod
{
    public void OnInitialize()
    {
        McRegistry.RegisterBlock("ruby_ore", new McBlock());
    }
}`
    }],

    events: [{
        name: 'EventDemo.cs',
        content: `using CSCraft;

[ModInfo("eventdemo", "Event Demo", "1.0.0")]
public class EventDemo : IMod
{
    public void OnInitialize()
    {
        Events.PlayerJoin += (player) =>
        {
            player.SendMessage("Hello " + player.Name + "!");
        };

        Events.PlayerLeave += (player) =>
        {
            McServer.BroadcastMessage(player.Name + " left.");
        };

        Events.PlayerDeath += (player) =>
        {
            player.SendMessage("You died! Better luck next time.");
        };

        Events.BlockBreak += (player, pos, state) =>
        {
            player.SendMessage("Broke a block at " + pos.X + ", " + pos.Y + ", " + pos.Z);
        };
    }
}`
    }],

    commands: [{
        name: 'CommandMod.cs',
        content: `using CSCraft;

[ModInfo("cmdmod", "Command Mod", "1.0.0")]
public class CommandMod : IMod
{
    public void OnInitialize()
    {
        McCommand.Register("heal", (player) =>
        {
            player.Health = player.MaxHealth;
            player.SendMessage("Healed!");
        });

        McCommand.Register("fly", (player) =>
        {
            player.SetFlySpeed(0.1f);
            player.SendMessage("Fly toggled!");
        });
    }
}`
    }],

    config: [{
        name: 'ConfigMod.cs',
        content: `using CSCraft;

[ModInfo("cfgmod", "Config Mod", "1.0.0")]
public class ConfigMod : IMod
{
    public void OnInitialize()
    {
        Events.PlayerJoin += (player) =>
        {
            string msg = Config.Get("welcome_message", "Welcome!");
            player.SendMessage(msg);
        };
    }
}`
    }],

    nbt: [{
        name: 'NbtDemo.cs',
        content: `using CSCraft;

// Note: GetNbtInt/SetNbtInt require Minecraft 1.21.2+
[ModInfo("nbtdemo", "NBT Demo", "1.0.0")]
public class NbtDemo : IMod
{
    public void OnInitialize()
    {
        Events.PlayerJoin += (player) =>
        {
            int visits = player.GetNbtInt("visit_count") + 1;
            player.SetNbtInt("visit_count", visits);
            player.SendMessage("Visit #" + visits);
        };
    }
}`
    }],

    multifile: [
        {
            name: 'MyMod.cs',
            content: `using CSCraft;

[ModInfo("multimod", "Multi-file Mod", "1.0.0")]
public class MyMod : IMod
{
    public void OnInitialize()
    {
        Events.PlayerJoin += (player) =>
        {
            Utils.Greet(player);
        };
    }
}`
        },
        {
            name: 'Utils.cs',
            content: `using CSCraft;

public class Utils
{
    public static void Greet(McPlayer player)
    {
        player.SendMessage("Hello from Utils, " + player.Name + "!");
    }
}`
        }
    ],
};

// ── File state ────────────────────────────────────────────────────────────────
let files      = [];   // [{ name: string, content: string }]
let activeFile = '';   // name of currently open file
let javaOutputs = {};  // { filename: javaSource }
let activeJavaTab = '';

// ── Monaco instances ──────────────────────────────────────────────────────────
let csEditor   = null;
let javaEditor = null;
let _monaco    = null;
let blazorReady = false;
let saveTimer   = null;
let ctxMenu     = null;  // open context menu element

// ── Storage key ───────────────────────────────────────────────────────────────
const LS = {
    FILES:   'cscraft_files',
    ACTIVE:  'cscraft_active',
    PKG:     'cscraft_package',
    VERSION: 'cscraft_version',
};

// ── Helpers ───────────────────────────────────────────────────────────────────
const $ = id => document.getElementById(id);

function getMcVersion() { return $('version-select').value || '1.21.1'; }
function getPackage()   { return $('package-input').value.trim() || 'com.example.mymod'; }
function getModId()     { return getPackage().split('.').pop().replace(/[^a-z0-9]/gi, '').toLowerCase() || 'mymod'; }

// ── Persist (auto-save) ───────────────────────────────────────────────────────
function scheduleSave() {
    clearTimeout(saveTimer);
    saveTimer = setTimeout(saveState, 800);
}

function saveState() {
    if (!files.length) return;
    try {
        localStorage.setItem(LS.FILES,   JSON.stringify(files));
        localStorage.setItem(LS.ACTIVE,  activeFile);
        localStorage.setItem(LS.PKG,     getPackage());
        localStorage.setItem(LS.VERSION, getMcVersion());
    } catch (_) { /* storage full */ }
}

function loadState() {
    // URL hash takes priority (shared link)
    if (window.location.hash) {
        try {
            const raw = LZString.decompressFromEncodedURIComponent(window.location.hash.slice(1));
            if (raw) {
                const parsed = JSON.parse(raw);
                if (parsed.files?.length) {
                    files      = parsed.files;
                    activeFile = parsed.active || files[0].name;
                    if (parsed.pkg)     $('package-input').value    = parsed.pkg;
                    if (parsed.version) $('version-select').value   = parsed.version;
                    return;
                }
            }
        } catch (_) { /* ignore */ }
        window.location.hash = '';
    }

    // LocalStorage
    try {
        const saved = localStorage.getItem(LS.FILES);
        if (saved) {
            files      = JSON.parse(saved);
            activeFile = localStorage.getItem(LS.ACTIVE) || files[0]?.name || '';
            const pkg  = localStorage.getItem(LS.PKG);
            const ver  = localStorage.getItem(LS.VERSION);
            if (pkg) $('package-input').value  = pkg;
            if (ver) $('version-select').value = ver;
            return;
        }
    } catch (_) { /* ignore */ }

    // Default
    files      = [...EXAMPLES.hello];
    activeFile = files[0].name;
}

// ── File tree ─────────────────────────────────────────────────────────────────
function renderFileTree() {
    const tree = $('file-tree');
    tree.innerHTML = '';
    files.forEach(f => {
        const item = document.createElement('div');
        item.className = 'file-item' + (f.name === activeFile ? ' active' : '');
        item.dataset.name = f.name;
        item.innerHTML = `
            <span class="file-icon">&#128196;</span>
            <span class="file-name" title="${f.name}">${f.name}</span>
            <button class="icon-btn file-menu" title="Options">&#8943;</button>`;
        item.querySelector('.file-name').addEventListener('click', () => switchFile(f.name));
        item.querySelector('.file-menu').addEventListener('click', e => { e.stopPropagation(); showCtxMenu(e, f.name); });
        item.addEventListener('contextmenu', e => { e.preventDefault(); showCtxMenu(e, f.name); });
        tree.appendChild(item);
    });
    $('active-filename').textContent = activeFile;
}

function switchFile(name) {
    if (activeFile && csEditor) {
        const f = files.find(x => x.name === activeFile);
        if (f) f.content = csEditor.getValue();
    }
    activeFile = name;
    const f = files.find(x => x.name === name);
    if (f && csEditor) {
        csEditor.setValue(f.content);
        csEditor.setScrollPosition({ scrollTop: 0 });
    }
    renderFileTree();
    updateJavaTabs();
    scheduleSave();
}

function addFile(name, content = '') {
    if (files.find(f => f.name === name)) {
        name = uniqueName(name);
    }
    files.push({ name, content });
    switchFile(name);
}

function uniqueName(base) {
    let n = 1;
    const ext  = base.includes('.') ? '.' + base.split('.').pop() : '';
    const stem = base.replace(/\.[^.]+$/, '');
    while (files.find(f => f.name === `${stem}${n}${ext}`)) n++;
    return `${stem}${n}${ext}`;
}

function deleteFile(name) {
    if (files.length === 1) { showToast('Cannot delete the only file.'); return; }
    files = files.filter(f => f.name !== name);
    delete javaOutputs[name];
    if (activeFile === name) switchFile(files[0].name);
    else renderFileTree();
    scheduleSave();
}

function renameFile(oldName, newName) {
    newName = newName.trim();
    if (!newName || newName === oldName) return;
    if (!newName.endsWith('.cs')) newName += '.cs';
    if (files.find(f => f.name === newName)) { showToast(`"${newName}" already exists.`); return; }
    const f = files.find(x => x.name === oldName);
    if (f) f.name = newName;
    if (activeFile === oldName) activeFile = newName;
    const java = javaOutputs[oldName];
    if (java !== undefined) { javaOutputs[newName] = java; delete javaOutputs[oldName]; }
    renderFileTree();
    updateJavaTabs();
    scheduleSave();
}

// ── Context menu ──────────────────────────────────────────────────────────────
function showCtxMenu(e, filename) {
    closeCtxMenu();
    const menu = document.createElement('div');
    menu.className = 'file-ctx-menu';
    menu.innerHTML = `
        <div class="file-ctx-item" data-action="rename">Rename</div>
        <div class="file-ctx-item" data-action="duplicate">Duplicate</div>
        <div class="file-ctx-item danger" data-action="delete">Delete</div>`;
    menu.style.left = e.clientX + 'px';
    menu.style.top  = e.clientY + 'px';
    menu.addEventListener('click', ev => {
        const action = ev.target.dataset.action;
        closeCtxMenu();
        if (action === 'rename')    showRenameDialog(filename);
        if (action === 'duplicate') duplicateFile(filename);
        if (action === 'delete')    deleteFile(filename);
    });
    document.body.appendChild(menu);
    ctxMenu = menu;
    setTimeout(() => document.addEventListener('click', closeCtxMenu, { once: true }), 0);
}

function closeCtxMenu() {
    if (ctxMenu) { ctxMenu.remove(); ctxMenu = null; }
}

function duplicateFile(name) {
    const f = files.find(x => x.name === name);
    if (!f) return;
    addFile(uniqueName(name), f.content);
}

// ── Rename dialog ─────────────────────────────────────────────────────────────
function showRenameDialog(name) {
    $('rename-dialog').style.display = 'flex';
    const input = $('rename-input');
    input.value = name;
    input.focus();
    input.select();

    const ok = () => {
        let val = input.value.trim();
        if (!val.endsWith('.cs')) val += '.cs';
        renameFile(name, val);
        $('rename-dialog').style.display = 'none';
    };
    $('rename-ok').onclick     = ok;
    $('rename-cancel').onclick = () => { $('rename-dialog').style.display = 'none'; };
    input.onkeydown = e => { if (e.key === 'Enter') ok(); if (e.key === 'Escape') $('rename-dialog').style.display = 'none'; };
}

// ── Java output tabs ──────────────────────────────────────────────────────────
function updateJavaTabs() {
    const tabsEl = $('java-tabs');
    tabsEl.innerHTML = '';
    const names = Object.keys(javaOutputs);
    if (names.length === 0) return;

    if (names.length === 1) {
        activeJavaTab = names[0];
        tabsEl.innerHTML = `<span class="java-tab active">${names[0].replace('.cs', '.java')}</span>`;
        return;
    }

    names.forEach(name => {
        const javaName = name.replace('.cs', '.java');
        const tab = document.createElement('span');
        tab.className = 'java-tab' + (name === activeJavaTab ? ' active' : '');
        tab.textContent = javaName;
        tab.addEventListener('click', () => {
            activeJavaTab = name;
            javaEditor.setValue(javaOutputs[name] || '');
            updateJavaTabs();
        });
        tabsEl.appendChild(tab);
    });
}

// ── Transpile ─────────────────────────────────────────────────────────────────
async function runTranspile() {
    if (!blazorReady) { log('info', 'Transpiler still loading…'); return; }

    // Flush current editor content
    if (activeFile && csEditor) {
        const f = files.find(x => x.name === activeFile);
        if (f) f.content = csEditor.getValue();
    }

    const pkg = getPackage();
    const btn = $('btn-run');
    const status = $('transpile-status');

    btn.disabled = true;
    btn.textContent = '⏳ Running…';
    status.className = 'status-tag';
    status.textContent = '';
    clearOutput();

    javaOutputs = {};
    let totalErrors = 0, totalWarnings = 0;

    try {
        for (const f of files) {
            const result = await DotNet.invokeMethodAsync('Playground', 'Transpile', f.content, pkg);
            javaOutputs[f.name] = result.java || '';
            if (result.errors?.length)   totalErrors   += result.errors.length;
            if (result.warnings?.length) totalWarnings += result.warnings.length;
            (result.errors   || []).forEach(e => log('err',  `[${f.name}:${e.line}] ${e.message}`));
            (result.warnings || []).forEach(w => log('warn', `[${f.name}:${w.line}] ${w.message}`));
        }

        // Show first file (or active file) in Java editor
        activeJavaTab = files.find(f => f.name === activeFile)?.name || files[0]?.name || '';
        javaEditor.setValue(javaOutputs[activeJavaTab] || '');
        javaEditor.setScrollPosition({ scrollTop: 0 });
        updateJavaTabs();

        if (totalErrors === 0 && totalWarnings === 0) {
            log('ok', `Transpiled ${files.length} file(s) successfully.`);
            status.className = 'status-tag ok';
            status.textContent = 'OK';
        } else if (totalErrors > 0) {
            status.className = 'status-tag err';
            status.textContent = `${totalErrors} error(s)`;
        } else {
            status.className = 'status-tag warn';
            status.textContent = `${totalWarnings} warning(s)`;
        }

    } catch (ex) {
        log('err', 'Runtime error: ' + ex);
        status.className = 'status-tag err';
        status.textContent = 'Error';
    } finally {
        btn.disabled    = false;
        btn.textContent = '▶ Run';
    }
}

// ── Download Fabric project zip ───────────────────────────────────────────────
async function downloadProject() {
    // Transpile first if outputs are empty
    if (Object.keys(javaOutputs).length === 0) {
        await runTranspile();
    }

    const pkg     = getPackage();
    const version = getMcVersion();
    const modId   = getModId();
    const vc      = MC_VERSIONS[version] || MC_VERSIONS['1.21.1'];
    const pkgPath = pkg.replace(/\./g, '/');

    // Detect the main mod class (implements IMod)
    const mainFile = files.find(f => f.content.includes('IMod')) || files[0];
    const mainClassName = mainFile
        ? (mainFile.name.replace('.cs', ''))
        : 'MyMod';
    const mainJavaFqn = `${pkg}.${mainClassName}`;

    const zip = new JSZip();
    const root = zip.folder(modId);

    // ── gradle files ──────────────────────────────────────────────────────────
    root.file('settings.gradle', `pluginManagement {
\trepositories {
\t\tmaven {
\t\t\tname = 'Fabric'
\t\t\turl = 'https://maven.fabricmc.net/'
\t\t}
\t\tmavenCentral()
\t\tgradlePluginPortal()
\t}
}
`);

    root.file('build.gradle', `buildscript {
\trepositories {
\t\tmaven {
\t\t\tname = 'Fabric'
\t\t\turl = 'https://maven.fabricmc.net/'
\t\t}
\t\tmavenCentral()
\t\tgradlePluginPortal()
\t}
\tdependencies {
\t\tclasspath "net.fabricmc:fabric-loom:${vc.loom}"
\t}
}

apply plugin: 'fabric-loom'
apply plugin: 'maven-publish'

version = project.mod_version
group   = project.maven_group

base {
\tarchivesName = project.archives_base_name
}

repositories {}

dependencies {
\tminecraft          "com.mojang:minecraft:\${project.minecraft_version}"
\tmappings           "net.fabricmc:yarn:\${project.yarn_mappings}:v2"
\tmodImplementation  "net.fabricmc:fabric-loader:\${project.loader_version}"
\tmodImplementation  "net.fabricmc.fabric-api:fabric-api:\${project.fabric_api_version}"
}

processResources {
\tinputs.property "version", project.version
\tfilesMatching("fabric.mod.json") { expand "version": inputs.properties.version }
}

tasks.withType(JavaCompile).configureEach {
\tit.options.release = ${vc.java}
}

java {
\twithSourcesJar()
\tsourceCompatibility = JavaVersion.VERSION_${vc.java}
\ttargetCompatibility = JavaVersion.VERSION_${vc.java}
}

jar {
\tinputs.property "archivesName", project.base.archivesName
\tfrom("LICENSE") { rename { "\${it}_\${inputs.properties.archivesName}" } }
}
`);

    root.file('gradle.properties', `# Gradle
org.gradle.jvmargs=-Xmx1G
org.gradle.parallel=true

# Fabric
minecraft_version=${version}
yarn_mappings=${vc.yarn}
loader_version=${vc.loader}
loom_version=${vc.loom}

# Mod
mod_version=1.0.0
maven_group=${pkg}
archives_base_name=${modId}

# Dependencies
fabric_api_version=${vc.fabric_api}
`);

    // ── fabric.mod.json ───────────────────────────────────────────────────────
    root.file('src/main/resources/fabric.mod.json', JSON.stringify({
        schemaVersion: 1,
        id:            modId,
        version:       '${version}',
        name:          mainClassName,
        description:   `Generated by CSCraft Playground`,
        authors:       [''],
        license:       'MIT',
        environment:   '*',
        entrypoints: {
            main: [mainJavaFqn]
        },
        mixins: [`${modId}.mixins.json`],
        depends: {
            fabricloader: `>=${vc.loader}`,
            minecraft:    `~${version}`,
            java:         `>=${vc.java}`,
            'fabric-api': '*'
        }
    }, null, '\t'));

    root.file(`src/main/resources/${modId}.mixins.json`, JSON.stringify({
        required:           true,
        package:            `${pkg}.mixin`,
        compatibilityLevel: `JAVA_${vc.java}`,
        mixins:             [],
        injectors:          { defaultRequire: 1 }
    }, null, '\t'));

    // ── Java source files ─────────────────────────────────────────────────────
    for (const f of files) {
        const javaName = f.name.replace('.cs', '.java');
        const javaSource = javaOutputs[f.name] || `// ${javaName} — run transpiler to generate\n`;
        root.file(`src/main/java/${pkgPath}/${javaName}`, javaSource);
    }

    // ── README ────────────────────────────────────────────────────────────────
    root.file('README.md', `# ${mainClassName}

Generated by [CSCraft Playground](https://github.com/BorkoAXT/CSCraft).

## Build

Requires Java ${vc.java}+ and Gradle.

\`\`\`sh
./gradlew build
\`\`\`

The compiled \`.jar\` will be in \`build/libs/\`.

## Install

Copy the \`.jar\` to your Minecraft \`mods/\` folder along with Fabric Loader ${vc.loader}
and Fabric API ${vc.fabric_api} for Minecraft ${version}.
`);

    // ── Generate & download ───────────────────────────────────────────────────
    const blob = await zip.generateAsync({ type: 'blob', compression: 'DEFLATE' });
    const url  = URL.createObjectURL(blob);
    const a    = document.createElement('a');
    a.href     = url;
    a.download = `${modId}-fabric-${version}.zip`;
    a.click();
    URL.revokeObjectURL(url);
    showToast(`Downloaded ${modId}-fabric-${version}.zip`);
}

// ── Share ─────────────────────────────────────────────────────────────────────
function shareUrl() {
    if (activeFile && csEditor) {
        const f = files.find(x => x.name === activeFile);
        if (f) f.content = csEditor.getValue();
    }
    const payload = JSON.stringify({ files, active: activeFile, pkg: getPackage(), version: getMcVersion() });
    const hash    = LZString.compressToEncodedURIComponent(payload);
    const url     = location.origin + location.pathname + '#' + hash;
    navigator.clipboard.writeText(url)
        .then(() => showToast('Shareable URL copied!'))
        .catch(() => window.prompt('Copy this URL:', url));
}

// ── Output panel ──────────────────────────────────────────────────────────────
function log(kind, msg) {
    const out  = $('console-output');
    const line = document.createElement('span');
    line.className = 'msg-' + kind;
    line.textContent = msg;
    out.appendChild(line);
    out.scrollTop = out.scrollHeight;
}

function clearOutput() { $('console-output').innerHTML = ''; }

function switchTab(name) {
    document.querySelectorAll('.panel-tab').forEach(t => t.classList.toggle('active', t.dataset.tab === name));
    $('panel-diag') .style.display = name === 'diag'  ? '' : 'none';
    $('panel-about').style.display = name === 'about' ? '' : 'none';
}

// ── Toast ─────────────────────────────────────────────────────────────────────
let _toastTimer = null;
function showToast(msg) {
    const t = $('toast');
    t.textContent = msg;
    t.classList.add('show');
    clearTimeout(_toastTimer);
    _toastTimer = setTimeout(() => t.classList.remove('show'), 2400);
}

// ── Blazor bootstrap ──────────────────────────────────────────────────────────
function startBlazor() {
    document.body.insertAdjacentHTML('beforeend', `
        <div id="blazor-loading">
            <div class="spinner"></div>
            <div class="loading-text">Loading transpiler…</div>
        </div>`);

    Blazor.start().then(() => {
        blazorReady = true;
        const el = $('blazor-loading');
        if (el) { el.classList.add('hidden'); setTimeout(() => el.remove(), 500); }
        log('ok', 'Transpiler ready.');
    }).catch(err => log('err', 'Failed to load transpiler: ' + err));
}

// ── Drag-to-resize dividers ───────────────────────────────────────────────────
function initDividers() {
    // Center divider (CS ↔ Java)
    makeDivider($('divider'), $('pane-cs'), null, 'horizontal');
    // Sidebar divider
    makeDivider($('sidebar-resize'), $('sidebar'), null, 'horizontal-sidebar');
}

function makeDivider(handle, paneA, _paneB, mode) {
    let dragging = false, startPos = 0, startSize = 0;

    handle.addEventListener('mousedown', e => {
        dragging  = true;
        startPos  = e.clientX;
        startSize = paneA.getBoundingClientRect().width;
        handle.classList.add('dragging');
        document.body.style.cursor     = 'col-resize';
        document.body.style.userSelect = 'none';
        e.preventDefault();
    });

    document.addEventListener('mousemove', e => {
        if (!dragging) return;
        const delta = e.clientX - startPos;
        const min   = mode === 'horizontal-sidebar' ? 120 : 200;
        const newW  = Math.max(min, startSize + delta);
        paneA.style.flex  = 'none';
        paneA.style.width = newW + 'px';
        csEditor?.layout();
        javaEditor?.layout();
    });

    document.addEventListener('mouseup', () => {
        if (!dragging) return;
        dragging = false;
        handle.classList.remove('dragging');
        document.body.style.cursor     = '';
        document.body.style.userSelect = '';
    });
}

// ── Main entry (Monaco ready) ─────────────────────────────────────────────────
require(['vs/editor/editor.main'], async function (monaco) {
    _monaco = monaco;

    loadState();

    const commonOpts = {
        theme:             'vs-dark',
        fontSize:          14,
        fontFamily:        "'Cascadia Code', 'Consolas', 'Courier New', monospace",
        fontLigatures:     true,
        minimap:           { enabled: false },
        scrollBeyondLastLine: false,
        lineNumbers:       'on',
        wordWrap:          'off',
        automaticLayout:   true,
        tabSize:           4,
        insertSpaces:      true,
        renderLineHighlight: 'line',
    };

    // C# editor
    const startFile = files.find(f => f.name === activeFile) || files[0];
    csEditor = monaco.editor.create($('editor-cs'), {
        ...commonOpts,
        language: 'csharp',
        value:    startFile?.content || '',
    });

    // Java output editor (read-only)
    javaEditor = monaco.editor.create($('editor-java'), {
        ...commonOpts,
        language: 'java',
        value:    '',
        readOnly: true,
        contextmenu: false,
    });

    // Auto-save on edit
    csEditor.onDidChangeModelContent(() => {
        const f = files.find(x => x.name === activeFile);
        if (f) f.content = csEditor.getValue();
        scheduleSave();
    });

    // Intellisense
    if (typeof initIntellisense === 'function') {
        await initIntellisense(monaco, csEditor);
    }

    // Keybindings
    csEditor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.Enter, () => runTranspile());

    // Render UI
    renderFileTree();

    // Wire buttons
    $('btn-run')     .addEventListener('click', () => runTranspile());
    $('btn-download').addEventListener('click', () => downloadProject());
    $('btn-share')   .addEventListener('click', () => shareUrl());
    $('btn-copy-java').addEventListener('click', () => {
        const java = javaEditor.getValue();
        if (!java) { showToast('No output yet.'); return; }
        navigator.clipboard.writeText(java).then(() => showToast('Java copied!'));
    });

    // New file
    $('btn-new-file').addEventListener('click', () => {
        addFile(uniqueName('NewClass.cs'), 'using CSCraft;\n\npublic class NewClass\n{\n    \n}\n');
    });

    // Upload .cs files
    $('btn-upload-file').addEventListener('click', () => $('file-upload-input').click());
    $('file-upload-input').addEventListener('change', async function () {
        for (const file of this.files) {
            const text = await file.text();
            addFile(file.name, text);
        }
        this.value = '';
    });

    // Examples
    $('examples-select').addEventListener('change', function () {
        const key = this.value;
        if (key && EXAMPLES[key]) {
            files      = EXAMPLES[key].map(f => ({ ...f }));
            activeFile = files[0].name;
            javaOutputs = {};
            csEditor.setValue(files[0].content);
            csEditor.setScrollPosition({ scrollTop: 0 });
            javaEditor.setValue('');
            $('java-tabs').innerHTML = '';
            $('transpile-status').textContent = '';
            renderFileTree();
            clearOutput();
            log('info', `Loaded example: ${key}`);
            scheduleSave();
        }
        this.value = '';
    });

    // Panel tabs
    document.querySelectorAll('.panel-tab').forEach(tab =>
        tab.addEventListener('click', () => switchTab(tab.dataset.tab)));
    $('panel-clear').addEventListener('click', clearOutput);

    // Version / package changes → re-save
    $('version-select').addEventListener('change', scheduleSave);
    $('package-input') .addEventListener('input',  scheduleSave);

    // Dividers
    initDividers();

    // Start Blazor
    startBlazor();
});

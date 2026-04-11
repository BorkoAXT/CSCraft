/**
 * playground.js — CSCraft online interpreter
 * Sets up Monaco editors, wires Blazor WASM transpiler, handles share/examples.
 */
'use strict';

// ── State ─────────────────────────────────────────────────────────────────────

let csEditor   = null;
let javaEditor = null;
let blazorReady = false;
let runPending  = false;     // queued run while Blazor loads

// ── Example snippets ──────────────────────────────────────────────────────────

const EXAMPLES = {
    hello: `using CSCraft;

[ModInfo("hellomod", "Hello Mod", "1.0.0")]
public class HelloMod : IMod
{
    public void OnInitialize()
    {
        Events.PlayerJoin += (player) =>
        {
            player.SendMessage("Welcome to the server, " + player.Name + "!");
        };
    }
}`,

    item: `using CSCraft;

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
}`,

    block: `using CSCraft;

[ModInfo("stonemod", "Stone Mod", "1.0.0")]
public class StoneMod : IMod
{
    public void OnInitialize()
    {
        McRegistry.RegisterBlock("ruby_ore", new McBlock());
    }
}`,

    events: `using CSCraft;

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
            McServer.BroadcastMessage(player.Name + " left the game.");
        };

        Events.PlayerDeath += (player) =>
        {
            player.SendMessage("You died! Better luck next time.");
        };

        Events.BlockBreak += (player, pos, state) =>
        {
            player.SendMessage("You broke a block at " + pos.X + ", " + pos.Y + ", " + pos.Z);
        };
    }
}`,

    commands: `using CSCraft;

[ModInfo("cmdmod", "Command Mod", "1.0.0")]
public class CommandMod : IMod
{
    public void OnInitialize()
    {
        McCommand.Register("heal", (player) =>
        {
            player.Health = player.MaxHealth;
            player.SendMessage("You have been healed!");
        });

        McCommand.Register("fly", (player) =>
        {
            player.SetFlySpeed(0.1f);
            player.SendMessage("Fly mode toggled!");
        });
    }
}`,

    config: `using CSCraft;

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
}`,

    nbt: `using CSCraft;

// Note: GetNbtString / SetNbtString require Minecraft 1.21.2+
[ModInfo("nbtdemo", "NBT Demo", "1.0.0")]
public class NbtDemo : IMod
{
    public void OnInitialize()
    {
        Events.PlayerJoin += (player) =>
        {
            int visits = player.GetNbtInt("visit_count") + 1;
            player.SetNbtInt("visit_count", visits);
            player.SendMessage("You have visited " + visits + " time(s).");
        };
    }
}`,
};

// ── Monaco setup ──────────────────────────────────────────────────────────────

require(['vs/editor/editor.main'], async function (monaco) {
    // Register C# language basics (Monaco only bundles a few languages by default)
    // Rely on Monaco's built-in 'csharp' TextMate grammar from the CDN bundle.

    const commonOptions = {
        theme:            'vs-dark',
        fontSize:         14,
        fontFamily:       "'Cascadia Code', 'Consolas', 'Courier New', monospace",
        fontLigatures:    true,
        minimap:          { enabled: false },
        scrollBeyondLastLine: false,
        renderLineHighlight: 'line',
        lineNumbers:      'on',
        wordWrap:         'off',
        automaticLayout:  true,
        tabSize:          4,
        insertSpaces:     true,
    };

    // C# input editor
    csEditor = monaco.editor.create(document.getElementById('editor-cs'), {
        ...commonOptions,
        language:     'csharp',
        value:        getInitialCode(),
        suggestOnTriggerCharacters: true,
    });

    // Java output editor (read-only)
    javaEditor = monaco.editor.create(document.getElementById('editor-java'), {
        ...commonOptions,
        language:  'java',
        value:     '',
        readOnly:  true,
        contextmenu: false,
    });

    // Wire up intellisense (defined in intellisense.js)
    if (typeof initIntellisense === 'function') {
        await initIntellisense(monaco, csEditor);
    }

    // Ctrl+Enter → run
    csEditor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.Enter, () => runTranspile());

    // Wire buttons
    document.getElementById('btn-run')  .addEventListener('click', () => runTranspile());
    document.getElementById('btn-share').addEventListener('click', () => shareUrl());
    document.getElementById('btn-copy') .addEventListener('click', () => copyJava());

    // Example selector
    document.getElementById('examples-select').addEventListener('change', function () {
        const key = this.value;
        if (key && EXAMPLES[key]) {
            csEditor.setValue(EXAMPLES[key]);
            csEditor.setScrollPosition({ scrollTop: 0 });
            javaEditor.setValue('');
            clearOutput();
            log('info', `Loaded example: ${key}`);
        }
        this.value = '';
    });

    // Panel tabs
    document.querySelectorAll('.panel-tab').forEach(tab => {
        tab.addEventListener('click', () => switchTab(tab.dataset.tab));
    });
    document.getElementById('panel-clear').addEventListener('click', clearOutput);

    // Drag-to-resize divider
    initDivider();

    // Start Blazor
    startBlazor();
});

// ── Initial code (from URL hash or default) ───────────────────────────────────

function getInitialCode() {
    try {
        const hash = window.location.hash.slice(1);
        if (hash) {
            const decoded = LZString.decompressFromEncodedURIComponent(hash);
            if (decoded) return decoded;
        }
    } catch (_) { /* ignore */ }
    return EXAMPLES.hello;
}

// ── Blazor WASM bootstrap ─────────────────────────────────────────────────────

function startBlazor() {
    // Show loading indicator
    const loadingHtml = `
        <div id="blazor-loading">
            <div class="loading-spinner"></div>
            <div class="loading-text">Loading transpiler…</div>
        </div>`;
    document.body.insertAdjacentHTML('beforeend', loadingHtml);

    Blazor.start({
        loadBootResource: (type, name, defaultUri, integrity) => defaultUri,
    }).then(() => {
        blazorReady = true;
        const overlay = document.getElementById('blazor-loading');
        if (overlay) {
            overlay.classList.add('hidden');
            setTimeout(() => overlay.remove(), 500);
        }
        log('ok', 'Transpiler ready.');
        if (runPending) { runPending = false; runTranspile(); }
    }).catch(err => {
        log('err', 'Failed to load transpiler: ' + err);
    });
}

// ── Transpile ─────────────────────────────────────────────────────────────────

async function runTranspile() {
    if (!blazorReady) {
        runPending = true;
        log('info', 'Waiting for transpiler to load…');
        return;
    }

    const csCode      = csEditor.getValue();
    const packageName = document.getElementById('package-input').value.trim() || 'com.example.mymod';
    const btn         = document.getElementById('btn-run');
    const status      = document.getElementById('transpile-status');

    btn.disabled  = true;
    btn.textContent = '⏳ Running…';
    status.textContent = 'Transpiling…';
    clearOutput();

    try {
        const result = await DotNet.invokeMethodAsync('Playground', 'Transpile', csCode, packageName);

        javaEditor.setValue(result.java || '');
        javaEditor.setScrollPosition({ scrollTop: 0 });

        const errCount  = result.errors  ?.length ?? 0;
        const warnCount = result.warnings?.length ?? 0;

        if (errCount === 0 && warnCount === 0) {
            log('ok', 'Transpilation successful.');
            status.textContent = 'OK';
        } else {
            if (errCount  > 0) status.textContent = `${errCount} error(s)`;
            else               status.textContent = `${warnCount} warning(s)`;
        }

        (result.errors   || []).forEach(e => log('err',  `[${e.line}] ${e.message}`));
        (result.warnings || []).forEach(w => log('warn', `[${w.line}] ${w.message}`));

        // Switch to Output tab if there are diagnostics
        if (errCount + warnCount > 0) switchTab('diag');

    } catch (ex) {
        log('err', 'Runtime error: ' + ex);
        status.textContent = 'Error';
    } finally {
        btn.disabled    = false;
        btn.textContent = '▶ Run';
    }
}

// ── Share ─────────────────────────────────────────────────────────────────────

function shareUrl() {
    const code       = csEditor.getValue();
    const compressed = LZString.compressToEncodedURIComponent(code);
    const url        = window.location.origin + window.location.pathname + '#' + compressed;

    navigator.clipboard.writeText(url)
        .then(() => showToast('Shareable URL copied!'))
        .catch(() => {
            // Fallback: prompt
            window.prompt('Copy this URL:', url);
        });
}

// ── Copy Java ─────────────────────────────────────────────────────────────────

function copyJava() {
    const java = javaEditor.getValue();
    if (!java) { showToast('Nothing to copy yet.'); return; }
    navigator.clipboard.writeText(java)
        .then(() => showToast('Java code copied!'))
        .catch(() => window.prompt('Copy Java:', java));
}

// ── Output panel ──────────────────────────────────────────────────────────────

function log(kind, msg) {
    const out  = document.getElementById('console-output');
    const line = document.createElement('span');
    line.className = 'msg-' + kind;
    line.textContent = msg;
    out.appendChild(line);
    out.scrollTop = out.scrollHeight;
}

function clearOutput() {
    document.getElementById('console-output').innerHTML = '';
}

function switchTab(name) {
    document.querySelectorAll('.panel-tab').forEach(t => t.classList.toggle('active', t.dataset.tab === name));
    document.getElementById('panel-diag') .style.display = name === 'diag'  ? '' : 'none';
    document.getElementById('panel-about').style.display = name === 'about' ? '' : 'none';
}

// ── Toast ─────────────────────────────────────────────────────────────────────

let toastTimer = null;
function showToast(msg) {
    const t = document.getElementById('toast');
    t.textContent = msg;
    t.classList.add('show');
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => t.classList.remove('show'), 2200);
}

// ── Drag-to-resize divider ────────────────────────────────────────────────────

function initDivider() {
    const divider   = document.getElementById('divider');
    const container = document.getElementById('editors-container');
    const paneLeft  = document.getElementById('pane-cs');
    const paneRight = document.getElementById('pane-java');

    let dragging = false, startX = 0, startLeftW = 0;

    divider.addEventListener('mousedown', e => {
        dragging   = true;
        startX     = e.clientX;
        startLeftW = paneLeft.getBoundingClientRect().width;
        divider.classList.add('dragging');
        document.body.style.cursor    = 'col-resize';
        document.body.style.userSelect = 'none';
        e.preventDefault();
    });

    document.addEventListener('mousemove', e => {
        if (!dragging) return;
        const totalW = container.getBoundingClientRect().width;
        const delta  = e.clientX - startX;
        const newW   = Math.max(200, Math.min(totalW - 200 - 5, startLeftW + delta));
        paneLeft.style.flex  = 'none';
        paneLeft.style.width = newW + 'px';
        paneRight.style.flex = '1';
        // Trigger Monaco layout update
        csEditor?.layout();
        javaEditor?.layout();
    });

    document.addEventListener('mouseup', () => {
        if (!dragging) return;
        dragging = false;
        divider.classList.remove('dragging');
        document.body.style.cursor    = '';
        document.body.style.userSelect = '';
    });
}

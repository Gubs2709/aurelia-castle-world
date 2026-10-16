import { LANDMARKS, type Quality } from './config';

const compassIcon = '<svg viewBox="0 0 32 32" fill="none" aria-hidden="true"><path d="M16 2v5m0 18v5M2 16h5m18 0h5" stroke="currentColor"/><circle cx="16" cy="16" r="11" stroke="currentColor"/><path d="m20 11-2 8-7 3 3-8 6-3Z" fill="currentColor"/></svg>';
export class UI {
  discovered = new Set<string>();
  started = false;
  paused = false;
  menu = false;
  location = 'The southern meadows';
  onStart = () => {};
  onReset = () => {};
  onPause = (_paused: boolean) => {};
  onQuality = (_quality: Quality) => {};
  noticeTimer = 0;
  constructor() {
    document.querySelector('#app')!.innerHTML = `
      <header class="topbar">
        <a class="brand" href="#" aria-label="Aurelia home">${compassIcon}<span>AURELIA<small>THE CASTLE VALLEY</small></span></a>
        <div class="top-center"><span class="status-dot"></span> AN OPEN WORLD TO WANDER</div>
        <button class="menu-button" id="settings-button" aria-label="Open settings">Settings <span>☷</span></button>
      </header>
      <section id="intro" class="intro">
        <div class="eyebrow"><span></span> A LITTLE WONDER. A GRAND ADVENTURE.</div>
        <h1>Somewhere<br>worth getting <em>lost.</em></h1>
        <p>Beyond the river, beneath the ancient trees,<br>a forgotten kingdom is waiting for you.</p>
        <button id="begin" class="primary" disabled><span id="begin-label">Preparing the valley</span><span class="arrow">↗</span></button>
        <div class="loading-track"><span id="loading-progress"></span></div>
        <div id="load-status" class="load-status" role="status">Gathering a little magic…</div>
        <div class="intro-details"><span>01 — AURELIA VALLEY</span><span>NO HURRY. JUST DISCOVER.</span></div>
      </section>
      <div class="chapter" id="chapter"><span>YOUR FIRST CHAPTER</span><strong>The road to Aurelia</strong><p>Four places. A thousand little details.</p></div>
      <div id="discovery" class="discovery" role="status"><span>NEW PLACE DISCOVERED</span><h2></h2><p></p></div>
      <aside id="map" class="map-panel"><div class="map-heading"><span>THE VALLEY</span><span id="count">0 / 4</span></div>
        <svg class="map" viewBox="0 0 160 155" aria-label="Map of the valley"><path d="M12 40Q45 27 74 39T152 40" fill="none" stroke="#6eaaa4" stroke-width="13" opacity=".55" transform="translate(0 48)"/><path d="M80 143V25M80 67l38-5" stroke="#d5c491" stroke-width="2" stroke-dasharray="3 4" fill="none"/><path d="M68 25V12h6v5h12v-5h6v13Z" stroke="#d7cfad" fill="none"/>
        ${LANDMARKS.map(l => `<circle id="map-${l.id}" cx="${80 + l.x * .65}" cy="${70 + l.z * .55}" r="3" fill="#8d9e89"/>`).join('')}
        <g id="map-player"><circle r="6" fill="#e6c483" opacity=".2"/><path d="m0-4 3 7-3-1-3 1Z" fill="#f3d99f"/></g></svg>
        <span class="map-caption">Wander a little further</span>
      </aside>
      <footer class="bottom-bar"><div class="location"><span class="location-icon">◇</span><span><small>YOU ARE HERE</small><strong id="location">The southern meadows</strong></span></div>
        <div class="controls"><span><kbd>W</kbd><kbd>A</kbd><kbd>S</kbd><kbd>D</kbd> Move</span><i></i><span><kbd>SHIFT</kbd> Sprint</span><span><kbd>SPACE</kbd> Jump</span><i></i><span>Drag to look · Scroll to zoom</span></div>
        <span id="quality-badge" class="quality-badge">LOW <span>●</span></span>
      </footer>
      <div id="settings" class="modal" role="dialog" aria-modal="true" aria-labelledby="settings-title" hidden><section class="settings-card"><span class="eyebrow">MAKE YOURSELF AT HOME</span><h2 id="settings-title">A moment of rest.</h2>
        <label for="quality">Graphics quality</label><select id="quality"><option value="low">Low · laptop friendly</option><option value="medium">Medium · balanced</option><option value="high">High · GPU PC</option></select>
        <p class="settings-note">Quality changes scenery detail, shadows, and resolution. Your route stays the same.</p>
        <div class="settings-controls"><p><kbd>WASD</kbd> Walk in any direction</p><p><kbd>SHIFT</kbd> Sprint <kbd>SPACE</kbd> Jump</p><p>Drag mouse to look · Scroll to zoom</p><p><kbd>ESC</kbd> Pause / resume</p></div>
        <button id="resume" class="primary">Back to the valley <span>↗</span></button><button id="reset" class="secondary">Return to the starting point</button>
        <p class="credit">Knight by Quaternius · CC0<br>World inspired by your fantasy references.</p>
      </section></div>
      <div id="error" class="error" hidden role="alert"><strong>The valley couldn’t load.</strong><p id="error-message"></p><button onclick="location.reload()">Try again</button></div>
      <div id="notice" class="notice" role="status"></div>`;
    document.querySelector('#begin')!.addEventListener('click', () => {
      this.started = true; document.body.classList.add('playing');
      (document.querySelector('#intro') as HTMLElement).inert = true;
      document.querySelector('#intro')!.setAttribute('aria-hidden', 'true'); this.onStart();
    });
    document.querySelector('#settings-button')!.addEventListener('click', () => this.toggleMenu());
    document.querySelector('#resume')!.addEventListener('click', () => this.toggleMenu(false));
    document.querySelector('#reset')!.addEventListener('click', () => {
      this.onReset(); this.toggleMenu(false); this.notice('Back at the beginning. Your discoveries are kept.');
    });
    document.querySelector('#quality')!.addEventListener('change', e => {
      const quality = (e.target as HTMLSelectElement).value as Quality;
      this.onQuality(quality); document.querySelector('#quality-badge')!.innerHTML = `${quality.toUpperCase()} <span>●</span>`;
    });
    addEventListener('keydown', e => { if (e.code === 'Escape') this.toggleMenu(); });
    document.addEventListener('visibilitychange', () => { if (document.hidden && this.started && !this.menu) this.toggleMenu(true); });
  }
  progress(value: number, text: string) {
    (document.querySelector('#loading-progress') as HTMLElement).style.width = `${value}%`;
    document.querySelector('#load-status')!.textContent = text;
  }
  ready() {
    this.progress(100, 'Your journey begins whenever you’re ready.');
    (document.querySelector('#begin') as HTMLButtonElement).disabled = false;
    document.querySelector('#begin-label')!.textContent = 'Enter the valley';
  }
  toggleMenu(force?: boolean) {
    this.menu = force ?? !this.menu; this.paused = this.menu;
    (document.querySelector('#settings') as HTMLElement).hidden = !this.menu;
    this.onPause(this.menu);
  }
  update(x: number, z: number, yaw: number) {
    document.querySelector('#map-player')!.setAttribute('transform', `translate(${80 + x * .65} ${70 + z * .55}) rotate(${-yaw * 180 / Math.PI})`);
    if (!this.started) return;
    let location = 'The open meadows';
    for (const l of LANDMARKS) {
      if (Math.hypot(x - l.x, z - l.z) < l.radius) {
        location = l.name;
        if (!this.discovered.has(l.id)) {
          this.discovered.add(l.id);
          document.querySelector(`#map-${l.id}`)!.setAttribute('fill', '#f1d392');
          document.querySelector('#count')!.textContent = `${this.discovered.size} / 4`;
          const banner = document.querySelector('#discovery')!;
          banner.querySelector('h2')!.textContent = l.name; banner.querySelector('p')!.textContent = l.subtitle;
          banner.classList.remove('visible'); requestAnimationFrame(() => banner.classList.add('visible'));
          clearTimeout(this.noticeTimer); this.noticeTimer = window.setTimeout(() => banner.classList.remove('visible'), 4800);
          if (this.discovered.size === 4) this.notice('All four places discovered. The valley is yours to wander.');
        }
      }
    }
    if (location !== this.location) { this.location = location; document.querySelector('#location')!.textContent = location; }
  }
  notice(text: string) {
    const el = document.querySelector('#notice')!; el.textContent = text; el.classList.add('visible');
    setTimeout(() => el.classList.remove('visible'), 6000);
  }
  error(error: unknown) {
    (document.querySelector('#error') as HTMLElement).hidden = false;
    document.querySelector('#error-message')!.textContent = error instanceof Error ? error.message : String(error);
    document.querySelector('#load-status')!.textContent = 'Loading stopped. See the message below.';
  }
}

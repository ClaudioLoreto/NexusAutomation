import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  NicheFormModel,
  NICHE_FORM_DEFAULTS,
  NICHE_TYPES,
  OPENAI_TTS_VOICES
} from './models/nexus-models';

/**
 * Stub editor for the new niche customisation surface. Renders every knob
 * the API now exposes (language, tone, word counts, TTS voice/speed,
 * karaoke styling, GIF overlay) so the data shape can be validated end-to-end.
 *
 * This is intentionally a "form skeleton": the submit handler is not wired
 * to a real POST yet — see `onSave()` for the placeholder. The full Create
 * Niche flow will be implemented when the backend's POST /api/niches
 * endpoint lands.
 */
@Component({
  selector: 'app-niche-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <section class="space-y-6 max-w-4xl">
      <header class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-semibold">Create Niche</h1>
          <p class="text-sm text-slate-400 mt-1">
            Each niche is a complete typology preset — language, voice, karaoke styling, and overlay
            settings. The render engine reads every value here at job time, so two niches with
            different settings can run in parallel without code changes.
          </p>
        </div>
        <a routerLink="/" class="text-sm text-sky-400 hover:text-sky-300">&larr; back to dashboard</a>
      </header>

      <form (ngSubmit)="onSave()" class="space-y-8">

        <fieldset class="space-y-3 rounded border border-slate-800 bg-slate-900/40 p-4">
          <legend class="px-2 text-xs uppercase tracking-wider text-slate-400">Identity</legend>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Type
              <select [(ngModel)]="form.type" name="type"
                      class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm">
                <option *ngFor="let t of nicheTypes" [value]="t">{{ t }}</option>
              </select>
            </label>
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Display name
              <input [(ngModel)]="form.name" name="name" type="text"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm" />
            </label>
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Language code (BCP-47)
              <input [(ngModel)]="form.languageCode" name="languageCode" type="text" placeholder="en-US"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm" />
            </label>
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Queue priority
              <input [(ngModel)]="form.queuePriority" name="queuePriority" type="number" min="0"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm" />
            </label>
          </div>
        </fieldset>

        <fieldset class="space-y-3 rounded border border-slate-800 bg-slate-900/40 p-4">
          <legend class="px-2 text-xs uppercase tracking-wider text-slate-400">Script</legend>
          <label class="block text-xs uppercase tracking-wider text-slate-400">
            Tone
            <input [(ngModel)]="form.scriptTone" name="scriptTone" type="text"
                   placeholder="dramatic / casual brainrot / wholesome"
                   class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm" />
          </label>
          <div class="grid grid-cols-2 gap-3">
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Target word count
              <input [(ngModel)]="form.targetWordCount" name="targetWordCount" type="number" min="0"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm" />
            </label>
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Max words
              <input [(ngModel)]="form.maxWords" name="maxWords" type="number" min="0"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm" />
            </label>
          </div>
          <label class="block text-xs uppercase tracking-wider text-slate-400">
            Additional script instructions
            <textarea [(ngModel)]="form.additionalScriptInstructions" name="additionalScriptInstructions"
                      rows="3"
                      class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm">
            </textarea>
          </label>
        </fieldset>

        <fieldset class="space-y-3 rounded border border-slate-800 bg-slate-900/40 p-4">
          <legend class="px-2 text-xs uppercase tracking-wider text-slate-400">Voiceover</legend>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-3">
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              OpenAI TTS voice
              <select [(ngModel)]="form.ttsVoice" name="ttsVoice"
                      class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm">
                <option *ngFor="let v of openAiVoices" [value]="v">{{ v }}</option>
              </select>
            </label>
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              TTS speed
              <input [(ngModel)]="form.ttsSpeed" name="ttsSpeed" type="number" step="0.05" min="0.25" max="4"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm" />
            </label>
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              ElevenLabs voice ID (optional)
              <input [(ngModel)]="form.elevenLabsVoiceId" name="elevenLabsVoiceId" type="text"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm" />
            </label>
          </div>
          <label class="block text-xs uppercase tracking-wider text-slate-400">
            Music directory
            <input [(ngModel)]="form.musicDirectory" name="musicDirectory" type="text"
                   class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm" />
          </label>
        </fieldset>

        <fieldset class="space-y-3 rounded border border-slate-800 bg-slate-900/40 p-4">
          <legend class="px-2 text-xs uppercase tracking-wider text-slate-400">Karaoke styling</legend>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-3">
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Font family
              <input [(ngModel)]="form.karaokeFontFamily" name="karaokeFontFamily" type="text"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm" />
            </label>
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Base size
              <input [(ngModel)]="form.karaokeFontSize" name="karaokeFontSize" type="number" min="20"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm" />
            </label>
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Highlight size
              <input [(ngModel)]="form.karaokeHighlightFontSize" name="karaokeHighlightFontSize" type="number" min="20"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm" />
            </label>
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Fill color
              <input [(ngModel)]="form.karaokeFillColor" name="karaokeFillColor" type="color"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm h-9" />
            </label>
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Highlight color
              <input [(ngModel)]="form.karaokeHighlightColor" name="karaokeHighlightColor" type="color"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm h-9" />
            </label>
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Outline color
              <input [(ngModel)]="form.karaokeOutlineColor" name="karaokeOutlineColor" type="color"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm h-9" />
            </label>
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Background color
              <input [(ngModel)]="form.karaokeBackgroundColor" name="karaokeBackgroundColor" type="color"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm h-9" />
            </label>
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Y position % (0=top, 100=bottom)
              <input [(ngModel)]="form.karaokeYPositionPercent" name="karaokeYPositionPercent" type="number"
                     step="0.5" min="0" max="100"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm" />
            </label>
          </div>
        </fieldset>

        <fieldset class="space-y-3 rounded border border-slate-800 bg-slate-900/40 p-4">
          <legend class="px-2 text-xs uppercase tracking-wider text-slate-400">Subscribe overlay</legend>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              GIF asset path
              <input [(ngModel)]="form.overlayGifPath" name="overlayGifPath" type="text"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm" />
            </label>
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Y position %
              <input [(ngModel)]="form.overlayGifPositionPercent" name="overlayGifPositionPercent" type="number"
                     step="0.5" min="0" max="100"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm" />
            </label>
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Tail seconds (when overlay starts before end)
              <input [(ngModel)]="form.overlayGifTailSeconds" name="overlayGifTailSeconds" type="number"
                     step="0.5" min="0"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm" />
            </label>
            <label class="block text-xs uppercase tracking-wider text-slate-400">
              Loop count (-1 = infinite, 0 = once)
              <input [(ngModel)]="form.overlayGifLoopCount" name="overlayGifLoopCount" type="number" min="-1"
                     class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-sm" />
            </label>
          </div>
        </fieldset>

        <div class="flex items-center gap-3 pt-2">
          <label class="flex items-center gap-2 text-sm text-slate-300">
            <input [(ngModel)]="form.isActive" name="isActive" type="checkbox"
                   class="h-4 w-4 accent-sky-500" />
            Active
          </label>
          <button type="submit"
                  class="ml-auto px-4 py-1.5 rounded bg-sky-600 hover:bg-sky-500 text-sm">
            Save niche (stub)
          </button>
        </div>

        <div *ngIf="status()" class="text-xs"
             [class.text-emerald-400]="status()?.kind === 'ok'"
             [class.text-rose-400]="status()?.kind === 'err'">
          {{ status()?.text }}
        </div>
      </form>
    </section>
  `
})
export class NicheEditorComponent {
  protected readonly nicheTypes = NICHE_TYPES;
  protected readonly openAiVoices = OPENAI_TTS_VOICES;

  form: NicheFormModel = { ...NICHE_FORM_DEFAULTS };
  status = signal<{ kind: 'ok' | 'err'; text: string } | null>(null);

  onSave(): void {
    // Backend POST /api/niches doesn't exist yet — once it lands this
    // will call NexusApiService.createNiche(this.form). For now we just
    // confirm the in-memory model so the data shape can be inspected
    // during integration.
    this.status.set({
      kind: 'ok',
      text: `Niche "${this.form.name || '(unnamed)'}" assembled in-memory. Backend wiring pending.`
    });
  }
}

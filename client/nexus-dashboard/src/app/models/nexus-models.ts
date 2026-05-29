export type NicheType =
  | 'Finance'
  | 'TechAndAi'
  | 'LegalAndCourt'
  | 'StoriaAntica'
  | 'BrainrotFacts'
  | 'WholesomeAnimals';

export type VideoStatus =
  | 'Pending'
  | 'TrendAnalyzed'
  | 'Scripting'
  | 'MediaDownloaded'
  | 'Rendering'
  | 'Completed'
  | 'ErrorRequiresHuman';

export type VideoJobPhase =
  | 'Pending'
  | 'ScriptDone'
  | 'AudioDone'
  | 'MediaDone'
  | 'RenderDone'
  | 'Failed';

export type VideoAssetKind =
  | 'StoryblocksClip'
  | 'Voiceover'
  | 'KaraokeSubtitle'
  | 'BackgroundMusic'
  | 'FinalRender'
  | 'Thumbnail';

/**
 * Mirrors `NicheDto` on the API side. Every editorial + render-pipeline
 * knob lives here so the niche editor can drive a single round-trip
 * create-or-update, with no per-field endpoints.
 */
export interface Niche {
  id: number;
  type: NicheType;
  name: string;
  languageCode: string;
  scriptTone: string;
  targetWordCount: number | null;
  maxWords: number | null;
  ttsVoice: string;
  ttsSpeed: number;
  elevenLabsVoiceId: string;
  musicDirectory: string;
  karaokeFontFamily: string;
  karaokeFontSize: number;
  karaokeHighlightFontSize: number;
  karaokeFillColor: string;
  karaokeHighlightColor: string;
  karaokeOutlineColor: string;
  karaokeBackgroundColor: string;
  karaokeYPositionPercent: number;
  overlayGifPath: string;
  overlayGifPositionPercent: number;
  overlayGifTailSeconds: number;
  overlayGifLoopCount: number;
  additionalScriptInstructions: string;
  isActive: boolean;
  queuePriority: number;
  videoCount: number;
}

/**
 * Form-shape used by the in-progress niche editor. Same fields as
 * `Niche` minus the server-owned `id` and `videoCount`.
 */
export type NicheFormModel = Omit<Niche, 'id' | 'videoCount'>;

export interface Video {
  id: string;
  nicheType: NicheType;
  status: VideoStatus;
  title: string | null;
  scriptText: string | null;
  outputPath: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

/**
 * Mirrors the new `VideoJob` entity for the engine pipeline.
 */
export interface VideoJob {
  id: string;
  nicheId: number;
  topic: string;
  storyblocksQuery: string | null;
  phase: VideoJobPhase;
  title: string | null;
  description: string | null;
  finalOutputPath: string | null;
  renderedDurationSeconds: number | null;
  scriptTokens: number | null;
  ttsCharacters: number | null;
  renderSeconds: number | null;
  costUsd: number | null;
  retryCount: number;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  completedAtUtc: string | null;
}

export interface VideoAsset {
  id: string;
  videoJobId: string;
  kind: VideoAssetKind;
  path: string;
  mediaType: string | null;
  sizeBytes: number | null;
  durationSeconds: number | null;
  createdAtUtc: string;
}

export interface RenderError {
  id: string;
  videoJobId: string;
  phaseAtFailure: VideoJobPhase;
  errorCode: string;
  message: string;
  detail: string | null;
  createdAtUtc: string;
}

export interface DashboardSummary {
  totalVideos: number;
  countByStatus: Record<VideoStatus, number>;
  niches: Niche[];
  generatedAtUtc: string;
}

export interface QueueVideoRequest {
  nicheType: NicheType;
  title?: string | null;
  storyblocksQuery?: string | null;
}

export interface ScrapedMediaResult {
  filePath: string;
  title: string | null;
  tags: string[];
}

export const VIDEO_STATUSES: VideoStatus[] = [
  'Pending',
  'TrendAnalyzed',
  'Scripting',
  'MediaDownloaded',
  'Rendering',
  'Completed',
  'ErrorRequiresHuman'
];

export const VIDEO_JOB_PHASES: VideoJobPhase[] = [
  'Pending',
  'ScriptDone',
  'AudioDone',
  'MediaDone',
  'RenderDone',
  'Failed'
];

export const NICHE_TYPES: NicheType[] = [
  'Finance',
  'TechAndAi',
  'LegalAndCourt',
  'StoriaAntica',
  'BrainrotFacts',
  'WholesomeAnimals'
];

export const OPENAI_TTS_VOICES = ['alloy', 'echo', 'fable', 'onyx', 'nova', 'shimmer'] as const;
export type OpenAiTtsVoice = (typeof OPENAI_TTS_VOICES)[number];

/**
 * Default niche shape when scaffolding a brand-new typology in the editor.
 * Mirrors the FFmpeg pipeline defaults so a fresh niche renders correctly
 * with zero hand-tuning.
 */
export const NICHE_FORM_DEFAULTS: NicheFormModel = {
  type: 'BrainrotFacts',
  name: '',
  languageCode: 'en-US',
  scriptTone: '',
  targetWordCount: 130,
  maxWords: 150,
  ttsVoice: 'alloy',
  ttsSpeed: 1.0,
  elevenLabsVoiceId: '',
  musicDirectory: 'Assets/Music/Default',
  karaokeFontFamily: 'The Bold Font',
  karaokeFontSize: 96,
  karaokeHighlightFontSize: 140,
  karaokeFillColor: '#FFFFFF',
  karaokeHighlightColor: '#FFFF00',
  karaokeOutlineColor: '#000000',
  karaokeBackgroundColor: '#0D1321',
  karaokeYPositionPercent: 7.0,
  overlayGifPath: 'Assets/Overlays/subscribe.gif',
  overlayGifPositionPercent: 95.3,
  overlayGifTailSeconds: 5.0,
  overlayGifLoopCount: 0,
  additionalScriptInstructions: '',
  isActive: true,
  queuePriority: 100
};

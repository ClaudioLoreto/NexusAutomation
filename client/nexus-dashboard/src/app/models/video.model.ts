export enum VideoStatus {
  Pending = 'Pending',
  TrendAnalyzed = 'TrendAnalyzed',
  Scripting = 'Scripting',
  MediaDownloaded = 'MediaDownloaded',
  Rendering = 'Rendering',
  Completed = 'Completed',
  ErrorRequiresHuman = 'ErrorRequiresHuman'
}

export enum NicheType {
  Finance = 'Finance',
  TechAndAI = 'TechAndAI',
  LegalAndCourt = 'LegalAndCourt'
}

export interface Video {
  id: string;
  title: string;
  niche: NicheType;
  status: VideoStatus;
  scriptText?: string;
  mediaFilePath?: string;
  audioFilePath?: string;
  outputFilePath?: string;
  errorMessage?: string;
  createdAtUtc: string;
  completedAtUtc?: string;
}

export interface NicheConfig {
  id: string;
  nicheType: NicheType;
  displayName: string;
  scriptTone: string;
  voiceStyle: string;
  musicGenre: string;
  musicDirectoryPath: string;
  searchKeywords: string[];
  isActive: boolean;
  priority: number;
}

export interface DashboardStats {
  totalVideos: number;
  pendingVideos: number;
  completedVideos: number;
  errorVideos: number;
  renderingVideos: number;
  nicheVelocities: Record<string, number>;
  topNiche: string;
}

export interface CreateVideoRequest {
  title: string;
  niche: NicheType;
}

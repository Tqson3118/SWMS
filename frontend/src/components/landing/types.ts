export interface CorePillar {
  id: string;
  title: string;
  tagline: string;
  description: string;
  iconName: string;
  iconColor: string;
  features: string[];
}

export interface ComparisonRow {
  criterion: string;
  traditional: string;
  swms: string;
  highlighted?: boolean;
}

export interface CompetitorBenchmark {
  competitor: string;
  swmsAdvantage: string;
  referenceQuote: string;
}

export interface WorkflowStep {
  stepNumber: number;
  title: string;
  subtitle: string;
  description: string;
}

export interface KeyShortcut {
  keys: string[];
  action: string;
  context: "global" | "reading" | "split" | "review";
}

export interface LandingClientProps {
  onOpenAbout?: () => void;
  onOpenShortcuts?: (initialTab?: "all" | "global" | "reading" | "split" | "review") => void;
}

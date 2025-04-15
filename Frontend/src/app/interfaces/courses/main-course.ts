export interface MainCourse {
  latestStage ?: number;
  progress ?: number;
  id: number;
  title: string;
  description: any;
  position: number;
  videoUrl: any;
  questions: Question[];
}

interface Question {
  id: number;
  questionText: string;
  position: number;
  answers: Answer[];
}

interface Answer {
  id: number;
  answerText: string;
  position: number;
  isCorrect: boolean;
}

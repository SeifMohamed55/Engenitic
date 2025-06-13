export interface MainCourse {
  latestStage?: number;
  progress?: number;
  id: number;
  title: string;
  description: any;
  position: number;
  videoUrl: any;
  questions: Question[];
  reviewDTO: Review | null;
}

interface Review {
  reviewId: number;
  userId: number;
  courseId: number;
  content: string;
  updatedAt: string;
  rating: number;
  fullName: string;
  imageMetadata: {
    name: string;
    imageURL: string;
    hash: number;
    version: string;
  };
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

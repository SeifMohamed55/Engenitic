export interface CourseDetails {
  id: number;
  title: string;
  code: string;
  description: string;
  instructorName: string;
  instructorEmail: string;
  instructorPhone: string;
  requirements: string;
  stages: number;
  isEnrolled: boolean;
  image: Image;
  ratingStats: RatingStats;
}

interface Image {
  name: string;
  imageURL: string;
  hash: number;
  version: any;
}

interface RatingStats {
  totalCount: number;
  averageRating: number;
  breakdown: {
    [key: number]: {
      count: number;
      percentage: number;
    };
  };
}

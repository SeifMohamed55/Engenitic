export interface CourseDetails {
  id: number;
  title: string;
  code: string;
  description: string;
  instructorName: string;
  instructorEmail: string;
  instructorPhone: string;
  requirements: string;
  image: Image;
  stages: number;
  isEnrolled?: boolean;
}

export interface Image {
  name: string;
  imageURL: string;
  hash: number;
}

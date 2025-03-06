export interface InstructorCourse {
    id: number
    title: string
    code: string
    description: string
    instructorName: string
    requirements: string
    stages: number
    image: Image
}
interface Image {
    name: string
    imageURL: string
}
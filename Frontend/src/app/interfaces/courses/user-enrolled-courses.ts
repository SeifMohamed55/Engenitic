export interface UserEnrolledCourses {
    totalPages: number
    totalItems: number
    currentlyViewing: string
    paginatedList: PaginatedList[]
}

export interface PaginatedList {
    id: number
    enrolledAt: string
    currentStage: number
    totalStages: number
    isCompleted: boolean
    progress: number
    course: Course
    courseId: number
}

export interface Course {
    id: number
    title: string
    code: string
    description: string
    instructorName: string
    requirements: string
    stages: number
    image: Image
}

export interface Image {
    name: string
    imageURL: string
}
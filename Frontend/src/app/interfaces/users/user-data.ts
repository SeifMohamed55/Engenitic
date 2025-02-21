export interface UserData {
    id: number
    email: string
    userName: string
    phoneNumber: string
    phoneRegionCode: string
    image: Image
    banned: boolean
}

interface Image {
    name: string
    imageURL: string
}

export interface Login {
    id: number
    banned: boolean
    name: string
    roles: string[]
    validTo: string
    image: Image
    accessToken: string
}

interface Image {
    url: string
    name: string
}

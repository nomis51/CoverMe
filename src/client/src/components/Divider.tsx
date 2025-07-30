import {twMerge} from "tailwind-merge";

export interface DividerProps {
    className?: string
    vertical?: boolean
}

export function Divider({className, vertical}: DividerProps) {
    return (
        <span className={twMerge(className, vertical ? `
        h-[80%]
        w-1
        border-l
        border-l-neutral-300     
    ` : `
       w-[80%]
        h-1
        border-t
        border-t-neutral-300     
    `)}></span>
    )
}
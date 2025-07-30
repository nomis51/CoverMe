import {twMerge} from "tailwind-merge";

export interface TextFieldProps {
    className?: string
    inputClassName?: string
    placeholder?: string
}

export function TextField({className, inputClassName, placeholder}: TextFieldProps) {
    return (
        <div className={twMerge(
            className ?? "",
            "relative w-full"
        )}>
            <input type="text"
                   placeholder={placeholder ?? ""}
                   className={twMerge(
                       inputClassName ?? "",
                       `w-full
                        bg-white 
                       border 
                       border-neutral-300 
                       rounded 
                       placeholder-neutral-500
                       pl-6
                       pr-1
                       focus:ring-none
                       focus:outline-none
                       outline-0
                       `
                   )}/>
            <span className="material-icons-outlined 
                    absolute 
                    left-0 
                    top-1/2 
                    -translate-y-1/2 
                    text-neutral-500
                    scale-80">search</span>
        </div>
    )
}
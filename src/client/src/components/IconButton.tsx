import {twMerge} from "tailwind-merge";

export interface IconButtonProps {
    className?: string
    icon: string;
    iconClassName: string;
    active?: boolean;
    onClick?: () => void
}

export function IconButton({icon, iconClassName, className, active, onClick}: IconButtonProps) {
    return (
        <button type="button"
                onClick={() => onClick && onClick()}
                className={twMerge(
                    className,
                    `flex 
                    flex-row 
                    items-center 
                    justify-center 
                    cursor-pointer 
                    hover:bg-neutral-300
                    rounded`,
                    active ? "bg-neutral-200" : ""
                )}>
            <span className={twMerge(iconClassName, "cursor-pointer")}>
                {icon}
            </span>
        </button>
    )
}
import {twMerge} from "tailwind-merge";

export interface IconButtonProps {
    icon: string;
    iconClass?: string
}

export function IconButton({icon, iconClass}: IconButtonProps) {
    return (
        <button type="button"
                className="flex flex-row items-center justify-center cursor-pointer">
            <span className={twMerge("material-icons", iconClass ?? "", "text-neutral-700")}>{icon}</span>
        </button>
    )
}
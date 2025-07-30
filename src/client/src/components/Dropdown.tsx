import {twMerge} from "tailwind-merge";

export interface DropdownProps {
    className?: string;
    selectClassName?: string;
    items: {
        value?: any;
        text: string;
    }[];
    placeholder?: string;
}

export function Dropdown({items, placeholder, className, selectClassName}: DropdownProps) {
    return (
        <div className={twMerge(className ?? "", `relative`)}>
            <select className={twMerge(selectClassName ?? "", `
                    appearance-none
                    bg-gray-50 border 
                    border-gray-300
                    text-gray-900
                    text-sm
                    rounded
                    focus:ring-blue-500
                    focus:border-blue-500
                    block
                    w-full
                    py-1
                    pl-2
                    pr-8
                `)}
                    defaultValue={placeholder ?? "Select an option"}>
                <option disabled
                        hidden>
                    {placeholder ?? "Select an option"}
                </option>
                {items.map((item) => (
                    <option key={item.value ?? item.text}
                            value={item.value ?? item.text}>
                        {item.text}
                    </option>
                ))}
            </select>
            <span className={`
                material-icons
                absolute 
                right-2 
                top-1/2 
                -translate-y-1/2 
                pointer-events-none 
                text-gray-500
                rotate-90
            `}>arrow_right</span>
        </div>
    );
}
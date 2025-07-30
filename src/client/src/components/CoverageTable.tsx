import react from "react";
import {twMerge} from "tailwind-merge";
import {IconButton} from "./IconButton.tsx";
import {type CoverageItem, itemToIcon, typeToNumber} from "../models/CoverageItem.ts";

const HEADERS = ["Symbol", "Coverage"];

export interface TreeViewTableProps {
    items: CoverageItem[];
    setItems: (items: TreeViewTableProps["items"]) => void;
}

export function CoverageTable({items, setItems}: TreeViewTableProps) {
    function toggleItem(index: number) {
        if (index < 0 || index >= items.length) return;

        items[index].isExpanded = !items[index].isExpanded;

        if (!items[index].isExpanded) {
            for (let i = index + 1; i < items.length; ++i) {
                const item1 = typeToNumber(items[index].type);
                const item2 = typeToNumber(items[i].type);
                if (item2 <= item1) continue;

                items[i].isExpanded = false;
            }
        }

        setItems([...items]);
    }

    function canExpand(index: number) {
        if (index >= items.length - 1) return false;
        if (index < 0) return false;

        const item1 = typeToNumber(items[index].type);
        const item2 = typeToNumber(items[index + 1].type);
        return item1 < item2;
    }

    return (
        <table className="w-full border-collapse">
            <tbody>
            <tr className="border-b border-neutral-300">
                {HEADERS.map((header, i) => (
                    <th className="text-left"
                        key={i}>{header}</th>
                ))}
            </tr>
            {items.map((item, i) => (
                <react.Fragment key={i}>
                    {(item.type === "root" || (i !== 0 && items[i - 1].isExpanded)) &&
                        <tr key={i}>
                            <td className={twMerge(
                                `flex flex-row items-center gap-1 p-0.5`,
                            )}
                                style={{paddingLeft: `${(typeToNumber(item.type)) * 20}px`}}>
                                {canExpand(i) ?
                                    <IconButton icon={"arrow_right"}
                                                iconClassName={"material-icons"}
                                                onClick={() => toggleItem(i)}/> :
                                    <div className="w-[24px] h-1"></div>
                                }
                                <img src={`images/${itemToIcon(item)}.svg`}
                                     alt="icon"
                                     className="w-4 h-4"/>
                                <p>{item.type}</p>
                                <p>{item.symbol}</p>
                            </td>
                            <td className="p-0.5">
                                <div className="w-full h-6 bg-neutral-200 relative rounded overflow-hidden">
                                    <div className="h-full bg-lime-300"
                                         style={{width: `${item.coverage}%`}}></div>
                                    <span className="absolute inset-0 flex items-center justify-center text-xs font-medium">
                                        {item.coverage}%
                                    </span>
                                </div>
                            </td>
                        </tr>
                    }
                </react.Fragment>
            ))}
            </tbody>
        </table>
    )
}
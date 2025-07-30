import {IconButton} from "../components/IconButton.tsx";
import {Dropdown} from "../components/Dropdown.tsx";
import {Divider} from "../components/Divider.tsx";
import {TextField} from "../components/TextField.tsx";
import {useState} from "react";
import {CoverageTable} from "../components/CoverageTable.tsx";
import type {CoverageItem} from "../models/CoverageItem.ts";

const items = [
    {text: "A", value: 1},
    {text: "B", value: 2},
    {text: "C", value: 3},
];
const TABLE_ITEMS: CoverageItem[] = [
    {
        symbol: "Total",
        coverage: 100,
        isExpanded: true,
        type: "root",
    }, {
        symbol: "Project A",
        coverage: 100,
        type: "project",
    }, {
        symbol: "Namespace A",
        coverage: 100,
        type: "namespace",
    }, {
        symbol: "Class A",
        coverage: 56,
        type: "class",
    }, {
        symbol: "Method A",
        coverage: 100,
        type: "method",
    }, {
        symbol: "Method B",
        type: "method",
        coverage: 100
    }
]

export function HomePage() {
    const [tableItems, setTableItems] = useState(TABLE_ITEMS);

    return (
        <div className="home-page w-full h-full overflow-hidden flex flex-col">
            <div className="w-full h-fit flex flex-row gap-2 items-center p-1">
                <Dropdown items={items}/>
                <IconButton icon={"play_arrow"}
                            active={true}
                            iconClassName={"material-icons-outlined text-neutral-500 scale-80"}/>
                <IconButton icon={"build"}
                            iconClassName={"material-icons-outlined text-neutral-500 scale-80"}/>

                <Divider vertical={true}/>

                <IconButton icon={"save"}
                            iconClassName={"material-symbols-outlined text-neutral-500 scale-80"}/>
                <IconButton icon={"delete"}
                            iconClassName={"material-symbols-outlined text-neutral-500 scale-80"}/>

                <Divider vertical={true}/>

                <IconButton icon={"settings"}
                            iconClassName={"material-icons-outlined text-neutral-500 scale-80"}/>
                <IconButton icon={"format_ink_highlighter"}
                            iconClassName={"material-symbols-outlined text-neutral-500 scale-80"}/>

            </div>
            <div className="w-full h-fit flex flex-row items-center p-1">
                <TextField placeholder="Type to search"/>
            </div>
            <div className="w-full h-full flex flex-col p-1">
                <CoverageTable items={tableItems}
                               setItems={setTableItems}/>

            </div>
        </div>
    )
}
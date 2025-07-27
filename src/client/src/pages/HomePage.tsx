import {IconButton} from "../components/IconButton.tsx";

export function HomePage() {
    return (
        <div className="home-page w-full h-full overflow-hidden flex flex-col">
            <div className="w-full h-fit flex flex-row gap-2 items-center">
                <select>
                    <option>Option 1</option>
                </select>
                <IconButton icon={"home"}/>
            </div>
            <div className="w-full h-fit flex flex-row">
                <input type="text"
                       placeholder="Search"/>
            </div>
            <div className="w-full h-full flex flex-col">
                table
            </div>
        </div>
    )
}
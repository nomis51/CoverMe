export const NAMESPACE = 'coverageTable';

export class CoverageTable {
    public static namespace = NAMESPACE;

    private resizableDivs: HTMLDivElement[] = [];
    private currentColumn?: HTMLElement
    private nextColumn?: HTMLElement
    private pageX?: number
    private currentColumnWidth?: number
    private nextColumnWidth?: number

    public initialize({id}: { id: string }) {
        if (!id) return;

        const table = document.getElementById(id) as HTMLTableElement;
        if (!table) return;

        this.makeResizable(table);
    }

    public dispose() {
        this.resizableDivs.forEach(div => div.addEventListener("mousedown", this.onMouseDown.bind(this)));
        this.resizableDivs.forEach(div => div.addEventListener("mouseover", this.onMouseOver.bind(this)));
        this.resizableDivs.forEach(div => div.addEventListener("mouseout", this.onMouseOut.bind(this)));
        document.addEventListener("mousemove", this.onMouseMove.bind(this));
        document.addEventListener("mouseup", this.onMouseUp.bind(this));
    }

    private makeResizable(table: HTMLTableElement) {
        const rows = table.getElementsByTagName("tr");
        if (!rows || rows.length === 0) return;

        const headerRow = rows[0];
        if (headerRow.children.length === 0) return;

        const columns = headerRow.children;
        const tableHeight = table.offsetHeight;

        for (let i = 0; i < columns.length; ++i) {
            const div = this.createResizableDiv(tableHeight);
            this.resizableDivs.push(div);
            columns[i].appendChild(div);
            (columns[i] as any).style.position = "relative";
            this.setListeners(div);
        }
    }

    private createResizableDiv(height: number): HTMLDivElement {
        const div = document.createElement("div");
        div.style.top = "0";
        div.style.right = "0";
        div.style.width = "5px";
        div.style.position = "absolute";
        div.style.cursor = "col-resize";
        div.style.userSelect = "none";
        div.style.height = height + "px";
        return div;
    }

    private setListeners(div: HTMLDivElement) {
        div.addEventListener("mousedown", this.onMouseDown.bind(this));
        div.addEventListener("mouseover", this.onMouseOver.bind(this));
        div.addEventListener("mouseout", this.onMouseOut.bind(this));
        document.addEventListener("mousemove", this.onMouseMove.bind(this));
        document.addEventListener("mouseup", this.onMouseUp.bind(this));
    }

    private getPaddingDiff(column: HTMLElement): number {
        if (this.getStyleValue(column, "box-sizing") === "border-box") return 0;

        const paddingLeft = this.getStyleValue(column, "padding-left");
        const paddingRight = this.getStyleValue(column, "padding-right");
        return parseInt(paddingLeft) + parseInt(paddingRight);
    }

    private getStyleValue(element: HTMLElement, style: string): string {
        return window.getComputedStyle(element).getPropertyValue(style);
    }

    private onMouseUp(_: MouseEvent) {
        this.currentColumn = undefined;
        this.nextColumn = undefined;
        this.pageX = undefined;
        this.currentColumnWidth = undefined;
        this.nextColumnWidth = undefined;
    }

    private onMouseOut(e: MouseEvent) {
        (e.target as any).style.borderRight = "";
    }

    private onMouseMove(e: MouseEvent) {
        if (!this.currentColumn) return;

        const diffX = e.pageX - this.pageX!;
        if (this.nextColumn) {
            this.nextColumn.style.width = (this.nextColumnWidth! - diffX) + "px";
        }

        this.currentColumn.style.width = (this.currentColumnWidth! + diffX) + "px";
    }

    private onMouseOver(e: MouseEvent) {
        (e.target as any).style.borderRight = "2px solid gray";
    }

    private onMouseDown(e: MouseEvent) {
        this.currentColumn = (e.target as any).parentElement;
        this.nextColumn = this.currentColumn!.nextElementSibling as any;
        this.pageX = e.pageX;

        const padding = this.getPaddingDiff(this.currentColumn!);
        this.currentColumnWidth = this.currentColumn!.offsetWidth - padding;
        if (!this.nextColumn) return;

        this.nextColumnWidth = this.nextColumn.offsetWidth - padding;
    }
}
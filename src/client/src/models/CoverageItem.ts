export interface CoverageItem {
    filePath?: string;
    symbol: string;
    coverage: number;
    isExpanded?: boolean;
    type: "root" | "project" | "namespace" | "class" | "method" | "field" | "property";
    visibility?: "public" | "protected" | "private" | "internal" | "sealed";
}

export function typeToNumber(type: string): number {
    switch (type) {
        case "root":
            return 0;
        case "project":
            return 1;
        case "namespace":
            return 2;
        case "class":
            return 3;
        case "method":
        case "field":
        case "property":
            return 4;
        default:
            return 99;
    }
}

export function itemToIcon(item: CoverageItem) {
    if (item.type === "root") return "FolderClosed";
    if (item.type === "project") return "Application";
    if (item.type === "namespace") return "Namespace";
    if (item.type === "class") return "Class";
    if (item.type === "method") {
        if (item.visibility === "public") return "MethodPublic";
        if (item.visibility === "protected") return "MethodProtected";
        if (item.visibility === "private") return "MethodPrivate";
        if (item.visibility === "internal") return "MethodInternal";
        if (item.visibility === "sealed") return "MethodSealed";

        return "Method";
    }
    if (item.type === "field") {
        if (item.visibility === "public") return "FieldPublic";
        if (item.visibility === "protected") return "FieldProtected";
        if (item.visibility === "private") return "FieldPrivate";
        if (item.visibility === "internal") return "FieldInternal";
        if (item.visibility === "sealed") return "FieldSealed";

        return "Field";
    }
    if (item.type === "property") {
        if (item.visibility === "public") return "PropertyPublic";
        if (item.visibility === "protected") return "PropertyProtected";
        if (item.visibility === "private") return "PropertyPrivate";
        if (item.visibility === "internal") return "PropertyInternal";
        if (item.visibility === "sealed") return "PropertySealed";

        return "Property";
    }

    return "Field";
}
import {Intellij} from "./intellij";
import {CoverageTable} from "./coverageTable";

const FEATURES = [
    Intellij,
    CoverageTable,
];
const NAMESPACE = "coverme";

window[NAMESPACE] = {};

for (const feature of FEATURES) {
    window[NAMESPACE][feature.namespace] = new feature();
}
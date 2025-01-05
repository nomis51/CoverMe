import {Intellij} from "./intellij";

const FEATURES = [
    Intellij
];
const NAMESPACE = "coverme";

window[NAMESPACE] = {};

for (const feature of FEATURES) {
    window[NAMESPACE][feature.namespace] = new feature();
}
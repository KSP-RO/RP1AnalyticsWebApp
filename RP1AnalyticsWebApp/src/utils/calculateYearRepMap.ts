import { DateTime } from 'luxon';

const repToSubsidyConversion = 100;
const subsidyMultiplierForMax = 2;
const perYearMinSubsidyArr = Object.freeze([
    25000,
    30000,
    35000,
    40000,
    60000,
    80000,
    100000,
    125000,
    150000,
    200000,
    250000,
    300000,
    375000,
    450000,
    500000,
    550000,
    600000
]);

export function calculateYearRepMap(): Map <number, number> {
    const yearRepMap = new Map<number, number>();   // Key is Unix timestamp of the year start; value is rep cap at that point in time
    for(let i = 0; i <perYearMinSubsidyArr.length; i++) {
        const year = 1951 + i;
        const dt = DateTime.fromObject({ year: year, month: 1, day: 1 });
        const minSubsidy = perYearMinSubsidyArr[i];
        const maxSubsidy = minSubsidy * subsidyMultiplierForMax;
        yearRepMap.set(dt.toUnixInteger(), (maxSubsidy - minSubsidy) / repToSubsidyConversion);
    }

    return yearRepMap;
}

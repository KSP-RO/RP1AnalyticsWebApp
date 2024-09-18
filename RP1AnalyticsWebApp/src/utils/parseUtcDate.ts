import { DateTime } from 'luxon';

export function parseUtcDate(sDt: string) {
    return DateTime.fromISO(sDt, { zone: 'utc' });
}

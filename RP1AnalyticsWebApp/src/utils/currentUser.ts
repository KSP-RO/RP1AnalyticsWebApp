import { CurrentUser } from 'types';

// Instantiated in _Layout.cshtml. Can be null.
export const currentUser = (<any>window) as CurrentUser | null;

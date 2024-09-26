import { CurrentUser } from 'types';

// Instantiated in _Layout.cshtml. Can be null.
declare const currentUser: CurrentUser | null;
export default currentUser;

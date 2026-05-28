import { create } from 'zustand';

type LocalUriCacheState = {
  uris: Record<string, string>;
  setLocalUri: (objectKey: string, uri: string) => void;
  getLocalUri: (objectKey: string) => string | undefined;
};

export const useLocalUriCache = create<LocalUriCacheState>((set, get) => ({
  uris: {},
  setLocalUri: (objectKey, uri) =>
    set((state) => ({ uris: { ...state.uris, [objectKey]: uri } })),
  getLocalUri: (objectKey) => get().uris[objectKey],
}));

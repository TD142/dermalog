import { create } from 'zustand';

export type Photo = {
  id: string;
  uri: string;
  objectKey: string;
  capturedAt: number;
};

type PhotosState = {
  photos: Photo[];
  addPhoto: (uri: string, objectKey: string) => void;
  removePhoto: (id: string) => void;
};

export const usePhotosStore = create<PhotosState>((set) => ({
  photos: [],
  addPhoto: (uri, objectKey) =>
    set((state) => ({
      photos: [
        {
          id: `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
          uri,
          objectKey,
          capturedAt: Date.now(),
        },
        ...state.photos,
      ],
    })),
  removePhoto: (id) =>
    set((state) => ({ photos: state.photos.filter((p) => p.id !== id) })),
}));

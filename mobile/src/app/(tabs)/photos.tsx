import { ActivityIndicator, FlatList, Image, Pressable, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useRouter } from 'expo-router';
import { Camera } from 'lucide-react-native';

import { type Photo, usePhotos } from '@/features/photos/api';
import { ErrorState } from '@/features/photos/components/dashboard-states';
import { useLocalUriCache } from '@/features/photos/store';

type LocalUriResolver = (objectKey: string) => string | undefined;

const PhotosBody = ({
  isLoading,
  isError,
  photos,
  getLocalUri,
  onRetry,
}: {
  isLoading: boolean;
  isError: boolean;
  photos: Photo[];
  getLocalUri: LocalUriResolver;
  onRetry: () => void;
}) => {
  if (isLoading) {
    return (
      <View className="flex-1 items-center justify-center">
        <ActivityIndicator color="#3A453E" />
      </View>
    );
  }
  if (isError) {
    return (
      <View className="flex-1 items-center justify-center">
        <ErrorState onRetry={onRetry} />
      </View>
    );
  }
  if (photos.length === 0) {
    return (
      <View className="flex-1 items-center justify-center">
        <View className="bg-cream rounded-3xl px-8 py-10 items-center max-w-xs">
          <Camera size={28} color="#3A453E" strokeWidth={1.5} />
          <Text className="text-sage-900 text-lg font-display-medium mt-3">
            No photos yet
          </Text>
          <Text className="text-sage-700 text-center text-sm mt-1">
            Use the Capture tab to add your first photo.
          </Text>
        </View>
      </View>
    );
  }
  return (
    <FlatList
      data={photos}
      keyExtractor={(p) => p.id}
      numColumns={2}
      columnWrapperClassName="gap-3"
      contentContainerClassName="gap-3 pb-4"
      renderItem={({ item }) => {
        const displayUri = getLocalUri(item.objectKey) ?? item.url;
        return (
          <View className="flex-1 aspect-square rounded-2xl overflow-hidden bg-cream items-center justify-center">
            {displayUri ? (
              <Image source={{ uri: displayUri }} className="w-full h-full" />
            ) : (
              <Camera size={28} color="#8FA395" strokeWidth={1.5} />
            )}
          </View>
        );
      }}
    />
  );
};

const PhotosScreen = () => {
  const router = useRouter();
  const { data: photos = [], isLoading, isError, refetch } = usePhotos();
  const getLocalUri = useLocalUriCache((s) => s.getLocalUri);

  return (
    <SafeAreaView edges={['top']} className="flex-1 bg-sage-200">
      <View className="flex-1 px-6 pt-8">
        <Text className="text-4xl text-sage-900 font-display">Photos</Text>
        <Text className="text-base text-sage-800 mt-1 mb-4">
          Every photo you&apos;ve captured.
        </Text>

        <PhotosBody
          isLoading={isLoading}
          isError={isError}
          photos={photos}
          getLocalUri={getLocalUri}
          onRetry={refetch}
        />

        {photos.length >= 2 && (
          <Pressable
            onPress={() => router.navigate('/compare')}
            className="bg-sage-900 rounded-2xl py-4 px-6 mb-4 active:opacity-80"
          >
            <Text className="text-cream text-center text-lg font-display-medium">
              Compare two photos
            </Text>
          </Pressable>
        )}
      </View>
    </SafeAreaView>
  );
};

export default PhotosScreen;

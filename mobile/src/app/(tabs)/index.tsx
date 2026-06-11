import { ActivityIndicator, ScrollView, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { type Comparison, type Photo, useComparisons, usePhotos } from '@/features/photos/api';
import { ComparisonsList } from '@/features/photos/components/comparisons-list';
import {
  EmptyState,
  ErrorState,
  NoComparisonPrompt,
} from '@/features/photos/components/dashboard-states';
import { useLocalUriCache } from '@/features/photos/store';

const dateLabel = new Intl.DateTimeFormat('en-GB', {
  weekday: 'long',
  day: 'numeric',
  month: 'long',
}).format(new Date());

type LocalUriResolver = (objectKey: string) => string | undefined;

const DashboardBody = ({
  isLoading,
  isError,
  photos,
  comparisons,
  getLocalUri,
  onRetry,
}: {
  isLoading: boolean;
  isError: boolean;
  photos: Photo[];
  comparisons: Comparison[];
  getLocalUri: LocalUriResolver;
  onRetry: () => void;
}) => {
  if (isLoading) {
    return (
      <View className="flex-1 items-center justify-center py-16">
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
        <EmptyState />
      </View>
    );
  }
  return comparisons.length > 0 ? (
    <ComparisonsList comparisons={comparisons} getLocalUri={getLocalUri} />
  ) : (
    <NoComparisonPrompt photos={photos} />
  );
};

const HomeScreen = () => {
  const photosQuery = usePhotos();
  const comparisonsQuery = useComparisons();
  const getLocalUri = useLocalUriCache((s) => s.getLocalUri);

  const photos = photosQuery.data ?? [];
  const comparisons = comparisonsQuery.data ?? [];

  const isLoading = photosQuery.isLoading || comparisonsQuery.isLoading;
  const isError = photosQuery.isError || comparisonsQuery.isError;

  const retry = () => {
    if (photosQuery.isError) photosQuery.refetch();
    if (comparisonsQuery.isError) comparisonsQuery.refetch();
  };

  return (
    <SafeAreaView edges={['top']} className="flex-1 bg-sage-200">
      <ScrollView contentContainerClassName="px-6 pt-8 pb-4 grow">
        <Text className="text-sage-700 text-sm uppercase tracking-widest">
          {dateLabel}
        </Text>
        <Text className="text-5xl text-sage-900 font-display mt-1">Dermalog</Text>
        <Text className="text-base text-sage-800 mt-2 max-w-xs leading-snug">
          Track your skin over time. Capture, log, and see what changes.
        </Text>

        <View className="flex-1 mt-6">
          <DashboardBody
            isLoading={isLoading}
            isError={isError}
            photos={photos}
            comparisons={comparisons}
            getLocalUri={getLocalUri}
            onRetry={retry}
          />
        </View>
      </ScrollView>
    </SafeAreaView>
  );
};

export default HomeScreen;

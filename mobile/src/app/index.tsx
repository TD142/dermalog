import {
  ActivityIndicator,
  Image,
  Pressable,
  ScrollView,
  Text,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useRouter } from 'expo-router';
import { ArrowRight, Camera, CloudOff } from 'lucide-react-native';

import {
  type Comparison,
  type Photo,
  useLatestComparison,
  usePhotos,
} from '@/features/photos/api';
import { TrendChip } from '@/features/photos/components/trend-chip';
import { useLocalUriCache } from '@/features/photos/store';

const dateLabel = new Intl.DateTimeFormat('en-GB', {
  weekday: 'long',
  day: 'numeric',
  month: 'long',
}).format(new Date());

type LocalUriResolver = (objectKey: string) => string | undefined;

const Thumb = ({
  photo,
  label,
  getLocalUri,
}: {
  photo: Photo;
  label: string;
  getLocalUri: LocalUriResolver;
}) => {
  const uri = getLocalUri(photo.objectKey) ?? photo.url;
  return (
    <View className="flex-1">
      <Text className="text-sage-700 text-[10px] uppercase tracking-widest mb-1">
        {label}
      </Text>
      <View className="aspect-square rounded-2xl overflow-hidden bg-sage-100 items-center justify-center">
        {uri ? (
          <Image source={{ uri }} className="w-full h-full" />
        ) : (
          <Camera size={24} color="#8FA395" strokeWidth={1.5} />
        )}
      </View>
    </View>
  );
};

const TrendCard = ({
  comparison,
  getLocalUri,
}: {
  comparison: Comparison;
  getLocalUri: LocalUriResolver;
}) => (
  <View className="bg-cream rounded-3xl p-5">
    <View className="flex-row items-center justify-between mb-4">
      <Text className="text-sage-800 text-xs uppercase tracking-widest">
        This week
      </Text>
      <TrendChip trend={comparison.severityTrend} />
    </View>
    <View className="flex-row items-center gap-2">
      <Thumb photo={comparison.before} label="Before" getLocalUri={getLocalUri} />
      <ArrowRight size={20} color="#8FA395" strokeWidth={1.5} />
      <Thumb photo={comparison.after} label="After" getLocalUri={getLocalUri} />
    </View>
    <Text className="text-sage-900 text-base mt-4 leading-snug">
      {comparison.overallSummary}
    </Text>
  </View>
);

const PromptCard = ({ title, body }: { title: string; body: string }) => (
  <View className="bg-cream rounded-3xl p-5">
    <Text className="text-sage-900 text-lg font-display-medium">{title}</Text>
    <Text className="text-sage-700 text-sm mt-1 leading-snug">{body}</Text>
  </View>
);

const lastEntryLabel = (iso: string) => {
  const days = Math.floor((Date.now() - new Date(iso).getTime()) / 86_400_000);
  if (days <= 0) return 'Today';
  return `${days}d`;
};

const Stat = ({ value, label }: { value: string; label: string }) => (
  <View className="flex-1 items-center">
    <Text className="text-sage-900 text-3xl font-display">{value}</Text>
    <Text className="text-sage-700 text-[10px] uppercase tracking-widest mt-1 text-center">
      {label}
    </Text>
  </View>
);

const TrackingStats = ({ photos }: { photos: Photo[] }) => {
  const daysLogged = new Set(photos.map((p) => p.capturedAt.slice(0, 10))).size;
  return (
    <View>
      <Text className="text-sage-800 text-xs uppercase tracking-widest mb-2">
        Your tracking
      </Text>
      <View className="bg-cream rounded-3xl p-5 flex-row">
        <Stat value={String(photos.length)} label="Photos" />
        <Stat value={String(daysLogged)} label="Days logged" />
        <Stat value={lastEntryLabel(photos[0].capturedAt)} label="Last entry" />
      </View>
    </View>
  );
};

const EmptyState = () => (
  <View className="bg-cream rounded-3xl px-8 py-10 items-center max-w-xs self-center">
    <View className="w-14 h-14 rounded-full bg-sage-100 items-center justify-center mb-4">
      <Camera size={28} color="#3A453E" strokeWidth={1.5} />
    </View>
    <Text className="text-sage-900 text-lg font-display-medium">No entries yet</Text>
    <Text className="text-sage-700 text-center text-sm mt-1">
      Tap Capture to start tracking.
    </Text>
  </View>
);

const ErrorState = ({ onRetry }: { onRetry: () => void }) => (
  <View className="items-center self-center max-w-xs">
    <View className="bg-coral/20 rounded-3xl px-8 py-10 items-center">
      <View className="w-14 h-14 rounded-full bg-sage-100 items-center justify-center mb-4">
        <CloudOff size={28} color="#3A453E" strokeWidth={1.5} />
      </View>
      <Text className="text-sage-900 text-lg font-display-medium text-center">
        Can&apos;t reach the server
      </Text>
      <Text className="text-sage-700 text-center text-sm mt-1">
        Check your connection and try again.
      </Text>
    </View>
    <Pressable
      onPress={onRetry}
      className="mt-4 bg-sage-900 rounded-2xl py-3 px-10 active:opacity-80"
    >
      <Text className="text-cream text-center font-display-medium">Retry</Text>
    </Pressable>
  </View>
);

const ProgressSection = ({
  photos,
  comparison,
  getLocalUri,
}: {
  photos: Photo[];
  comparison: Comparison | null;
  getLocalUri: LocalUriResolver;
}) => {
  if (comparison) {
    return <TrendCard comparison={comparison} getLocalUri={getLocalUri} />;
  }
  if (photos.length >= 2) {
    return (
      <PromptCard
        title="See your progress"
        body="Compare two of your photos to track how things are changing over time."
      />
    );
  }
  return (
    <PromptCard
      title="One photo so far"
      body="Capture another photo of the same area to start comparing."
    />
  );
};

const DashboardBody = ({
  isLoading,
  isError,
  photos,
  comparison,
  getLocalUri,
  onRetry,
}: {
  isLoading: boolean;
  isError: boolean;
  photos: Photo[];
  comparison: Comparison | null;
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
  return (
    <View className="gap-4">
      <ProgressSection
        photos={photos}
        comparison={comparison}
        getLocalUri={getLocalUri}
      />
      <TrackingStats photos={photos} />
    </View>
  );
};

const HomeScreen = () => {
  const router = useRouter();
  const photosQuery = usePhotos();
  const latestQuery = useLatestComparison();
  const getLocalUri = useLocalUriCache((s) => s.getLocalUri);

  const photos = photosQuery.data ?? [];
  const comparison = latestQuery.data ?? null;

  const isLoading = photosQuery.isLoading || latestQuery.isLoading;
  const isError = photosQuery.isError || latestQuery.isError;
  const showCtas = !isLoading && !isError;

  const retry = () => {
    if (photosQuery.isError) photosQuery.refetch();
    if (latestQuery.isError) latestQuery.refetch();
  };

  return (
    <SafeAreaView className="flex-1 bg-sage-200">
      <View className="flex-1">
        <ScrollView
          className="flex-1"
          contentContainerClassName="px-6 pt-8 pb-4 grow"
        >
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
              comparison={comparison}
              getLocalUri={getLocalUri}
              onRetry={retry}
            />
          </View>
        </ScrollView>

        {showCtas && (
          <View className="mb-4 px-6 gap-3">
            <Pressable
              onPress={() => router.push('/capture')}
              className="bg-sage-900 rounded-2xl py-4 px-6 active:opacity-80"
            >
              <Text className="text-cream text-center text-lg font-display-medium">
                Capture
              </Text>
            </Pressable>
            {photos.length >= 2 && (
              <Pressable
                onPress={() => router.push('/compare')}
                className="border border-sage-900 rounded-2xl py-4 px-6 active:opacity-80"
              >
                <Text className="text-sage-900 text-center text-lg font-display-medium">
                  Compare
                </Text>
              </Pressable>
            )}
          </View>
        )}
      </View>
    </SafeAreaView>
  );
};

export default HomeScreen;

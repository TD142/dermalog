import { Text, View } from 'react-native';

import { type Comparison } from '@/features/photos/api';
import {
  ComparisonCard,
  type LocalUriResolver,
} from '@/features/photos/components/comparison-card';

export const ComparisonsList = ({
  comparisons,
  getLocalUri,
}: {
  comparisons: Comparison[];
  getLocalUri: LocalUriResolver;
}) => (
  <View className="gap-4">
    <Text className="text-sage-800 text-xs uppercase tracking-widest">
      Comparisons
    </Text>
    {comparisons.map((c) => (
      <ComparisonCard key={c.id} comparison={c} getLocalUri={getLocalUri} />
    ))}
  </View>
);

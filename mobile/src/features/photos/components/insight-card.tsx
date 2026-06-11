import { Text, View } from 'react-native';
import { Sparkles } from 'lucide-react-native';

import { type Insight } from '@/features/photos/api';

export const InsightCard = ({ insight }: { insight: Insight }) => (
  <View className="bg-cream rounded-3xl p-5">
    <View className="flex-row items-center gap-1.5 mb-2">
      <Sparkles size={14} color="#8FA395" strokeWidth={1.5} />
      <Text className="text-sage-800 text-xs uppercase tracking-widest">
        Your progress
      </Text>
    </View>
    <Text className="text-sage-900 text-lg font-display-medium leading-snug">
      {insight.headline}
    </Text>
    <Text className="text-sage-800 text-sm mt-1 leading-snug">{insight.body}</Text>
    <Text className="text-sage-700 text-[10px] mt-3 leading-snug">
      Based on {insight.basisComparisonCount} comparisons · a read of your own logs,
      not medical advice
    </Text>
  </View>
);

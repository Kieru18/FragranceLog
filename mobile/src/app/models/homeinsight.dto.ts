import { InsightScopeEnum } from '../enums/insightscope.enum';
import { HomeInsightIconEnum } from '../enums/homeinsighticon.enum';

export interface HomeInsightDto {
  key: string;
  title: string;
  subtitle: string;
  icon: HomeInsightIconEnum;
  scope: InsightScopeEnum;
}

// This code was auto-generated, any manual changes made will be lost.

import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";

export type SchedulingProviderBag = {
    attributes?: Record<string, PublicAttributeBag> | null;

    attributeValues?: Record<string, string> | null;

    description?: string | null;

    entityType?: ListItemBag | null;

    idKey?: string | null;

    isActive: boolean;

    name?: string | null;
};

export type SchedulingProviderDetailOptionsBag = {
};

export type SchedulingProviderListOptionsBag = {
};

export type SchedulingProviderLocationBag = {
    attributes?: Record<string, PublicAttributeBag> | null;

    attributeValues?: Record<string, string> | null;

    externalId?: string | null;

    id: number;

    idKey?: string | null;

    locationId?: number | null;

    schedulingProvider?: ListItemBag | null;

    schedulingProviderId?: number | null;
};

export type SchedulingProviderLocationListOptionsBag = {
    isBlockVisible: boolean;

    locationId?: string | null;

    locationName?: string | null;

    schedulingProviders?: ListItemBag[] | null;
};

export type SchedulingProviderReservationListOptionsBag = {
};

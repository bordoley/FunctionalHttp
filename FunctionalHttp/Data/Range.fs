namespace FunctionalHttp

type Range = | Range


type RangeUnit = | RangeUnit

type AcceptableRanges = 
    | RangeUnits of Set<RangeUnit>
    | AcceptsNone

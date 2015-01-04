namespace FunctionalHttp.Core

type Range = private | Range


type RangeUnit = private | RangeUnit

type AcceptableRanges = 
    private 
    | RangeUnits of Set<RangeUnit>
    | AcceptsNone

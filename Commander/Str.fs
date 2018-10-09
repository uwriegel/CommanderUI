module Str

let indexOf (str: string) (subStr: string) =
    str.IndexOf subStr

/// Gets the index of the first occurrence of chr in the string str
/// <param name="chr">The char for which to find the index</param>
/// <param name="start">Index in str where to start search</param>
/// <param name="str">The string where to search for index</param>
let indexOfStartPos (chr: char) (start: int) (str: string) = str.IndexOf (chr, start)

let substring startIndex (str: string) = str.Substring startIndex
using System;


namespace Emphasize; 

[Flags()]
public enum EmphasisType {
    None = 0,
    Bold = 1,
    Italic = 2,
    Code = 4,

}

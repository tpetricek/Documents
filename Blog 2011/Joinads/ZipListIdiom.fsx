// --------------------------------------------------------------------------------------

type ZipList() = 
  // Unit of ZipList idiom is infinite sequence of values
  member x.Return(a) = seq { while true do yield a }
  // Standard projection for sequences
  member x.Select(v, f) = Seq.map f v
  // Zip values from two (possibly infinite) sequences
  member x.Merge(u, v) = Seq.zip u v

// Create instance of the computation builder
let zip = ZipList()

// --------------------------------------------------------------------------------------

let rec transpose (matrix) = zip {
  if Seq.length matrix = 0 then 
    // Generate infinite sequence of empty lists
    return Seq.empty
  else 
    // Zip elements of the first row with rows of recursively 
    // transposed sub-matrix starting from the second row
    let! xs = Seq.head matrix
    and xss = transpose (Seq.skip 1 matrix)
    return Seq.concat [ seq [ xs ]; xss ] }


// Gives: [ [ 1; 4]; [2; 5]; [3; 6] ]
transpose [ [ 1; 2; 3]; [ 4; 5; 6 ] ]

// --------------------------------------------------------------------------------------

let rec transpose (matrix) = 
  // Two branches of 'if' are translated separately
  if Seq.length matrix = 0 then 
    zip.Return(Seq.empty)
  else 
    // Combine inputs using 'Merge' and then use 
    // 'Select' to implement projection
    zip.Select
      ( zip.Merge(Seq.head matrix, transpose (Seq.skip 1 matrix)),
        fun (xs, xss) -> Seq.concat [ seq [ xs ]; xss ])
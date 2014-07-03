namespace Stuff.EmailValidator

type IRequirement = 
  abstract IsSatisfied : string -> bool

module Requirements = 
  let LengthRequirement = 
    { new IRequirement with
        member x.IsSatisfied(password) = password.Length >= 8 }

  let DigitRequirement =
    { new IRequirement with
        member x.IsSatisfied(password) = password |> Seq.exists System.Char.IsDigit }
  
  let UpperCaseRequirement =
    { new IRequirement with
        member x.IsSatisfied(password) = password |> Seq.exists System.Char.IsUpper }

type PowerValidator(requirements:seq<IRequirement>) = 
  member x.IsValid(password) =
    requirements |> Seq.forall (fun x -> x.IsSatisfied password)
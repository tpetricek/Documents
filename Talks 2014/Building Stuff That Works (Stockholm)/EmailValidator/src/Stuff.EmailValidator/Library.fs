namespace Stuff.EmailValidator

/// Represents a password requirement
type IRequirement = 
  /// Should return `true` when the requirement is satisfied
  abstract IsSatisfied : string -> bool

/// Module that exposes built-in requirements including:
///
///  - Password length requirement
///  - Requirement to include digits
///  - Requirement to include upper case letters
///
module Requirements = 
  /// Requres that the password length is more than or equal to 8
  let LengthRequirement = 
    { new IRequirement with
        member x.IsSatisfied(password) = password.Length >= 8 }

  /// Requires that the password contains digits
  let DigitRequirement =
    { new IRequirement with
        member x.IsSatisfied(password) = password |> Seq.exists System.Char.IsDigit }
  
  /// Requires that the password contains upper case letter
  let UpperCaseRequirement =
    { new IRequirement with
        member x.IsSatisfied(password) = password |> Seq.exists System.Char.IsUpper }

/// Power validator that combines multiple validators
type PowerValidator(requirements:seq<IRequirement>) = 
  member x.IsValid(password) =
    requirements |> Seq.forall (fun x -> x.IsSatisfied password)
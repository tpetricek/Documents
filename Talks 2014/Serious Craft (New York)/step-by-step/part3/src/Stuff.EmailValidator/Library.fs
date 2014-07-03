namespace Stuff.EmailValidator

type IRequirement = 
  abstract IsSatisfied : string -> bool

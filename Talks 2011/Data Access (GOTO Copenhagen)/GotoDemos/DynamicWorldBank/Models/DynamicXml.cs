using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Xml.Linq;

namespace Goto.Dynamic
{
  class DynamicXml : DynamicObject
  {
    // An XML node that the current 'DynamicXml' object represents
    // (this can be either a root container or any node in the document)
    private XContainer container;

    // Additional properties - root namespace, flag specifying whether
    // names should be camelCased and a function for resolving names.
    private string ns;
    private bool camelCase;
    private Func<string, XName> nameResolver;

    /// <summary>
    /// Create a DynamicXml object from any LINQ to XML document or node.
    /// </summary>
    /// <param name="node">The node that should be wrapped.</param>
    /// <param name="Namespace">Global XML namespace to be used when resolving names.</param>
    /// <param name="CamelCase">When set to true, names (e.g. "xn.FooBar") will be turned to 
    /// camelCase (e.g. "xn.fooBar")</param>
    public DynamicXml(XContainer node, string Namespace = null, bool CamelCase = false)
    {
      this.camelCase = CamelCase;
      this.ns = Namespace;
      this.container = node;
      Func<string, string> camel = s => camelCase ? s.Substring(0, 1).ToLower() + s.Substring(1) : s;
      if (Namespace == null) nameResolver = n => XName.Get(camel(n));
      else nameResolver = n => XName.Get(camel(n), Namespace);
    }

    /// <summary>
    /// Create XML document from a specified URI (either local or web)
    /// </summary>
    /// <param name="node">The node that should be wrapped.</param>
    /// <param name="Namespace">Global XML namespace to be used when resolving names.</param>
    /// <param name="CamelCase">When set to true, names (e.g. "xn.FooBar") will be turned to 
    /// camelCase (e.g. "xn.fooBar")</param>
    public DynamicXml(string uri, string Namespace = null, bool CamelCase = false) :
      this(XDocument.Load(uri), Namespace, CamelCase) { }


    /// <summary>
    /// Called when the user accesses a property dynamically (e.g. "xn.Countries")
    /// </summary>
    /// <returns>Depending on the data, this may return a collection of nodes
    /// or a single node or a string value.</returns>
    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
      result = null;

      // First search for nested elements
      var elems = container.Elements(nameResolver(binder.Name));
      if (elems.Count() == 1)
        // Single element - wrap it inside DynamicXml
        result = new DynamicXml(elems.First(), ns, camelCase);
      else if (elems.Count() != 0)
      {
        // Multiple elements - wrapp all inside DynamicXml & return sequecne
        result = elems.Select(e => new DynamicXml(e, ns, camelCase));
      }
      else
      {
        // No sub-element found. If the current node is Element, try attributes
        var element = container as XElement;
        if (element != null)
          result = element.Attributes(nameResolver(binder.Name)).FirstOrDefault();

        // Special name 'Value' can be used to get the inner text.
        if (binder.Name == "Value") result = element.Value;
      }
      return result != null;
    }
  }
}

using PostSharp.Extensibility;
using PostSharp.Patterns.Diagnostics;

// This file contains registration of aspects that are applied to several classes of this project.
[assembly: Log(AttributeTargetTypeAttributes=MulticastAttributes.Private | MulticastAttributes.Protected | MulticastAttributes.Internal | MulticastAttributes.Public, AttributeTargetMemberAttributes=MulticastAttributes.Private | MulticastAttributes.Protected | MulticastAttributes.Internal | MulticastAttributes.Public | MulticastAttributes.UserGenerated)]
[assembly: Log(AttributePriority = 1)]
[assembly: Log(AttributeExclude = true, AttributeTargetMembers = "regex:^get_|^set_", AttributePriority = 2)]
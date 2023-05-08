using PostSharp.Extensibility;
using PostSharp.Patterns.Diagnostics;

// This file contains registration of aspects that are applied to several classes of this project.
[assembly:
    Log(
        AttributeTargetTypeAttributes = MulticastAttributes.Private |
                                        MulticastAttributes.Protected |
                                        MulticastAttributes.Internal |
                                        MulticastAttributes.Public,
        AttributeTargetMemberAttributes = MulticastAttributes.Private |
                                          MulticastAttributes.Protected |
                                          MulticastAttributes.Internal |
                                          MulticastAttributes.Public |
                                          MulticastAttributes.UserGenerated
    )]
[assembly: Log(AttributePriority = 1)]
[assembly: Log(AttributePriority = 2, AttributeExclude = true, AttributeTargetMembers = "get_*" )]
[assembly: Log(AttributePriority = 3, AttributeExclude = true, AttributeTargetMembers = "set_*" )]
[assembly: Log(AttributePriority = 4, AttributeExclude = true, AttributeTargetMembers = "add_*" )]
[assembly: Log(AttributePriority = 5, AttributeExclude = true, AttributeTargetMembers = "remove_*" )]
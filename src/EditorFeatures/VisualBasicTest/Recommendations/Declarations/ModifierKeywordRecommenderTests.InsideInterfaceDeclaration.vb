﻿' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Namespace Microsoft.CodeAnalysis.Editor.VisualBasic.UnitTests.Recommendations.Declarations.ModifierKeywordRecommenderTests
    <Trait(Traits.Feature, Traits.Features.KeywordRecommending)>
    Public Class InsideInterfaceDeclaration
        Inherits RecommenderTests

#Region "Scope Keywords"

        <Fact
#Region "Scope Keywords"
>
        Public Sub PublicDoesNotExistTest()
            VerifyRecommendationsMissing(<InterfaceDeclaration>|</InterfaceDeclaration>, "Public")
        End Sub

        <Fact>
        Public Sub ProtectedDoesNotExistTest()
            VerifyRecommendationsMissing(<InterfaceDeclaration>|</InterfaceDeclaration>, "Protected")
        End Sub

        <Fact>
        Public Sub PrivateDoesNotExistTest()
            VerifyRecommendationsMissing(<InterfaceDeclaration>|</InterfaceDeclaration>, "Private")
        End Sub

        <Fact>
        Public Sub FriendDoesNotExistTest()
            VerifyRecommendationsMissing(<InterfaceDeclaration>|</InterfaceDeclaration>, "Friend")
        End Sub

        <Fact>
        Public Sub ProtectedFriendDoesNotExistTest()
            VerifyRecommendationsMissing(<InterfaceDeclaration>|</InterfaceDeclaration>, "Protected Friend")
        End Sub

#End Region

#Region "Narrowing and Widening Keywords"

        <Fact
#Region "Narrowing and Widening Keywords"
>
        Public Sub NarrowingDoesNotExistTest()
            VerifyRecommendationsMissing(<InterfaceDeclaration>|</InterfaceDeclaration>, "Narrowing")
        End Sub

        <Fact>
        Public Sub WideningDoesNotExistTest()
            VerifyRecommendationsMissing(<InterfaceDeclaration>|</InterfaceDeclaration>, "Widening")
        End Sub

#End Region

#Region "MustInherit and NotInheritable Keywords"

        <Fact
#Region "MustInherit and NotInheritable Keywords"
>
        Public Sub MustInheritDoesExistTest()
            VerifyRecommendationsContain(<InterfaceDeclaration>|</InterfaceDeclaration>, "MustInherit")
        End Sub

        <Fact>
        Public Sub NotInheritableDoesExistTest()
            VerifyRecommendationsContain(<InterfaceDeclaration>|</InterfaceDeclaration>, "NotInheritable")
        End Sub

#End Region

#Region "Overrides and Overridable Set of Keywords"

        <Fact
#Region "Overrides and Overridable Set of Keywords"
>
        Public Sub OverridesDoesNotExistTest()
            VerifyRecommendationsMissing(<InterfaceDeclaration>|</InterfaceDeclaration>, "Overrides")
        End Sub

        <Fact>
        Public Sub MustOverrideDoesNotExistTest()
            VerifyRecommendationsMissing(<InterfaceDeclaration>|</InterfaceDeclaration>, "MustOverride")
        End Sub

        <Fact>
        Public Sub OverridableDoesNotExistTest()
            VerifyRecommendationsMissing(<InterfaceDeclaration>|</InterfaceDeclaration>, "Overridable")
        End Sub

        <Fact>
        Public Sub NotOverridableDoesNotExistTest()
            VerifyRecommendationsMissing(<InterfaceDeclaration>|</InterfaceDeclaration>, "NotOverridable")
        End Sub

        <Fact>
        Public Sub OverloadsDoesExistTest()
            VerifyRecommendationsContain(<InterfaceDeclaration>|</InterfaceDeclaration>, "Overloads")
        End Sub

#End Region

#Region "ReadOnly and WriteOnly Keywords"

        <Fact
#Region "ReadOnly and WriteOnly Keywords"
>
        Public Sub ReadOnlyDoesExistTest()
            VerifyRecommendationsContain(<InterfaceDeclaration>|</InterfaceDeclaration>, "ReadOnly")
        End Sub

        <Fact>
        Public Sub WriteOnlyDoesExistTest()
            VerifyRecommendationsContain(<InterfaceDeclaration>|</InterfaceDeclaration>, "WriteOnly")
        End Sub

#End Region

#Region "Partial Keyword"

        <Fact
#Region "Partial Keyword"
>
        Public Sub PartialDoesNotExistTest()
            VerifyRecommendationsMissing(<InterfaceDeclaration>|</InterfaceDeclaration>, "Partial")
        End Sub

#End Region

#Region "Shadows Keyword"

        <Fact
#Region "Shadows Keyword"
>
        Public Sub ShadowsDoesExistTest()
            ' This is actually allowed by the spec. Be careful: the MSDN documentation is wrong
            ' here.
            VerifyRecommendationsContain(<InterfaceDeclaration>|</InterfaceDeclaration>, "Shadows")
        End Sub

#End Region

#Region "Shared Keyword"

        <Fact
#Region "Shared Keyword"
>
        Public Sub SharedDoesNotExistTest()
            VerifyRecommendationsMissing(<InterfaceDeclaration>|</InterfaceDeclaration>, "Shared")
        End Sub

#End Region

        <Fact>
        <WorkItem(674791, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/674791")>
        Public Sub NotAfterHashTest()
            VerifyRecommendationsMissing(<File>
Imports System

#|
 
Module Module1
 
End Module

</File>, "Friend", "Private", "Protected", "Protected Friend", "Shadows", "Shared",
        "ReadOnly", "WriteOnly", "MustOverride", "Overridable", "Public", "Overloads", "Overrides", "WithEvents")
        End Sub
    End Class
End Namespace

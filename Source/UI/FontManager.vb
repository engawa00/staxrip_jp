Imports System.Drawing.Text
Imports System.Globalization
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text

Public NotInheritable Class FontManager
    Private Shared ReadOnly _currentFontFamilies As New Dictionary(Of FontCategory, FontFamily)
    Private Shared ReadOnly _fontCollections As New Dictionary(Of FontCategory, PrivateFontCollection)

    Public Const DefaultCodeFontSize As Single = 10.0
    Public Const DefaultFontSize As Single = 9.0
    Public Const DefaultThumbnailFontSize As Single = 10.0

    Public Shared Sub Init()
        For Each category As FontCategory In [Enum].GetValues(GetType(FontCategory)).Cast(Of FontCategory)().Skip(1)
            AddCollection(category)
        Next
    End Sub

    Shared Sub Reset()
        _currentFontFamilies.Clear()
    End Sub

    Public Shared Sub AddCollection(category As FontCategory)
        Dim collection As New PrivateFontCollection()
        Dim categoryDir = Path.Combine(Folder.Fonts, category.ToString())

        If Directory.Exists(categoryDir) Then
            Dim fontFiles = Directory.GetFiles(categoryDir, "*.ttf", SearchOption.AllDirectories)
            For Each fontFile In fontFiles
                Try
                    collection.AddFontFile(fontFile)
                Catch
                End Try
            Next
        End If

        If g.SettingsFolderExists Then
            If Folder.UserFonts.DirExists() Then
                Dim subfolderPath = Path.Combine(Folder.UserFonts, category.ToString())
                If subfolderPath.DirExists() Then
                    Dim fontFiles = Directory.GetFiles(subfolderPath, "*.ttf", SearchOption.AllDirectories)
                    For Each fontFile In fontFiles
                        Try
                            collection.AddFontFile(fontFile)
                        Catch
                        End Try
                    Next
                End If
            End If
        End If

        If collection.Families.Any() Then
            _fontCollections(category) = collection
        End If
    End Sub

    Shared Function GetFontFamilies(category As FontCategory, filtered As Boolean) As List(Of FontFamily)
        If Not _fontCollections.Any() Then Init()

        Dim collections = If(category = FontCategory.All, _fontCollections, _fontCollections.Where(Function(x) x.Key = category))
        Dim fontFamilies = collections.SelectMany(Function(x) x.Value.Families).ToList()

        If Not fontFamilies.Any() Then
            Try
                fontFamilies = FontFamily.Families.ToList()
            Catch
            End Try
        End If

        If filtered Then
            fontFamilies = fontFamilies _
                .Where(Function(x) Not x.Name.ContainsAny({"UltraCondensed", "UltraExpanded"})) _
                .Where(Function(x) Not x.Name.EndsWithAny({" Thin"})) _
                .ToList()
        End If

        Return fontFamilies
    End Function

    Shared Function GetFontFamily(category As FontCategory, fontName As String) As FontFamily
        If Not _fontCollections.Any() Then Init()
        Dim collections = If(category = FontCategory.All, _fontCollections, _fontCollections.Where(Function(x) x.Key = category))
        Dim found = collections.SelectMany(Function(s) s.Value.Families.Where(Function(x) x.Name.Equals(fontName, StringComparison.OrdinalIgnoreCase))).FirstOrDefault(Function(x) x IsNot Nothing)
        If found IsNot Nothing Then Return found

        If Not String.IsNullOrEmpty(fontName) Then
            Try
                Dim sys = FontFamily.Families.FirstOrDefault(Function(f) f.Name.Equals(fontName, StringComparison.OrdinalIgnoreCase))
                If sys IsNot Nothing Then Return sys
            Catch
            End Try
        End If

        Return Nothing
    End Function

    Shared Function GetFont(category As FontCategory, fontName As String, Optional size As Single = DefaultFontSize, Optional fontStyle As FontStyle = FontStyle.Regular, Optional graphicsUnit As GraphicsUnit = GraphicsUnit.Point, Optional gdiCharSet As Byte = 0) As Font
        If Not _fontCollections.Any() Then Init()

        Dim family = GetFontFamily(category, fontName)

        If family IsNot Nothing Then
            Return GetFont(family, size, fontStyle, graphicsUnit, gdiCharSet)
        End If

        If _fontCollections.Any() AndAlso _fontCollections.First().Value.Families.Any() Then
            Return New Font(_fontCollections.First().Value.Families.First(), size * s.UIScaleFactor, fontStyle, graphicsUnit, gdiCharSet)
        End If

        ' フォントコレクションが存在しない場合の安全なシステムフォントフォールバック
        Dim fallbackFamily = SystemFonts.MessageBoxFont?.FontFamily
        If fallbackFamily Is Nothing Then fallbackFamily = FontFamily.GenericSansSerif
        Dim actualScale = If(s IsNot Nothing, s.UIScaleFactor, 1.0F)
        Return New Font(fallbackFamily, size * actualScale, fontStyle, graphicsUnit, gdiCharSet)
    End Function

    Shared Function GetFont(fontFamily As FontFamily, Optional size As Single = DefaultFontSize, Optional fontStyle As FontStyle = FontStyle.Regular, Optional graphicsUnit As GraphicsUnit = GraphicsUnit.Point, Optional gdiCharSet As Byte = 0) As Font
        Dim actualFamily = If(fontFamily, SystemFonts.MessageBoxFont?.FontFamily)
        If actualFamily Is Nothing Then actualFamily = FontFamily.GenericSansSerif
        Dim actualScale = If(s IsNot Nothing, s.UIScaleFactor, 1.0F)
        Return New Font(actualFamily, size * actualScale, fontStyle, graphicsUnit, gdiCharSet)
    End Function

    Shared Function GetCodeFont(Optional sizeOffset As Single = 0.0, Optional fontStyle As FontStyle = FontStyle.Regular) As Font
        Dim family As FontFamily
        Dim size = DefaultCodeFontSize + sizeOffset

        If _currentFontFamilies.TryGetValue(FontCategory.Code, family) Then
            Return GetFont(family, size, fontStyle)
        End If

        Dim fontName = If(s?.Fonts?.ContainsKey(FontCategory.Code) = True, s.Fonts(FontCategory.Code), "Consolas")
        Dim font = GetFont(FontCategory.Code, fontName, size, fontStyle)
        _currentFontFamilies(FontCategory.Code) = font.FontFamily
        Return font
    End Function

    Shared Function GetDefaultFont(Optional sizeOffset As Single = 0.0, Optional fontStyle As FontStyle = FontStyle.Regular) As Font
        Dim family As FontFamily
        Dim size = DefaultFontSize + sizeOffset

        If _currentFontFamilies.TryGetValue(FontCategory.Default, family) Then
            Return GetFont(family, size, fontStyle)
        End If

        Dim fontName = If(s?.Fonts?.ContainsKey(FontCategory.Default) = True, s.Fonts(FontCategory.Default), "")
        Dim font = GetFont(FontCategory.Default, fontName, size, fontStyle)
        _currentFontFamilies(FontCategory.Default) = font.FontFamily
        Return font
    End Function

    Shared Function GetThumbnailFont(Optional sizeOffset As Single = 0.0, Optional fontStyle As FontStyle = FontStyle.Regular) As Font
        Dim family As FontFamily
        Dim size = DefaultThumbnailFontSize + sizeOffset

        If _currentFontFamilies.TryGetValue(FontCategory.Thumbnail, family) Then
            Return GetFont(family, size, fontStyle)
        End If

        Dim fontName = If(s?.Fonts?.ContainsKey(FontCategory.Thumbnail) = True, s.Fonts(FontCategory.Thumbnail), "")
        Dim font = GetFont(FontCategory.Thumbnail, fontName, size, fontStyle)
        _currentFontFamilies(FontCategory.Thumbnail) = font.FontFamily
        Return font
    End Function
End Class

Public Enum FontCategory
    All
    Code
    [Default]
    Thumbnail
End Enum
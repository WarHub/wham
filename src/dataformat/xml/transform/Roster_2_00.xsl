<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" 
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
                xmlns:bsr="http://www.battlescribe.net/schema/rosterSchema" 
                xmlns="http://www.battlescribe.net/schema/rosterSchema"
                exclude-result-prefixes="bsr">

    <xsl:output method="xml" indent="yes"/>

    
    <xsl:template match="/bsr:roster">
        <roster>
            <!-- Attributes -->
            <xsl:attribute name="battleScribeVersion">2.00</xsl:attribute>
            <xsl:apply-templates select="@*[name(.) != 'battleScribeVersion' and name(.) != 'points' and name(.) != 'pointsLimit']" />
            
            
            <!-- Nodes -->
            <xsl:choose>
                <xsl:when test="bsr:description">
                    <customNotes>
                        <xsl:value-of select="bsr:description" />
                    </customNotes>
                </xsl:when>
            </xsl:choose>
            
            <xsl:if test="@pointsLimit and @pointsLimit != 0">
                <costLimits>
                    <costLimit costTypeId="points" name="pts">
                        <xsl:attribute name="value"><xsl:value-of select="@pointsLimit"/></xsl:attribute>
                    </costLimit>
                </costLimits>
            </xsl:if>
            
            <xsl:apply-templates select="node()[name(.) != 'bsr:customDescription']"/>
        </roster>
    </xsl:template>
    
    
    <xsl:template match="bsr:force">
        <force>
            <xsl:attribute name="entryId"><xsl:value-of select="@forceTypeId" /></xsl:attribute>
            <xsl:attribute name="name"><xsl:value-of select="@forceTypeName" /></xsl:attribute>
            
            <xsl:apply-templates select="@*[name(.) != 'forceTypeId' and name(.) != 'forceTypeName' and name(.) != 'name'] | node()"/>
        </force>
    </xsl:template>
    
    
    <xsl:template match="bsr:category">
        <category>
            <xsl:attribute name="entryId"><xsl:value-of select="@categoryId" /></xsl:attribute>
            
            <xsl:apply-templates select="@*[name(.) != 'categoryId'] | node()"/>
        </category>
    </xsl:template>

    
    <xsl:template match="bsr:selection">
        <selection>
            <!-- Attributes -->
            <xsl:apply-templates select="@*[name(.) != 'points']"/>
            
            
            <!-- Nodes -->
            <xsl:choose>
                <xsl:when test="bsr:customDescription">
                    <customNotes>
                        <xsl:value-of select="bsr:customDescription" />
                    </customNotes>
                </xsl:when>
            </xsl:choose>
            
            <xsl:apply-templates select="node()[name(.) != 'bsr:customDescription']"/>
        </selection>
    </xsl:template>

    
    <!-- Recreated by app -->
    <xsl:template match="bsr:profile"></xsl:template>
    
    <!-- Recreated by app -->
    <xsl:template match="bsr:rule"></xsl:template>
    
    
    <xsl:template match="* | bsr:*">
        <xsl:element name="{local-name(.)}">
            <xsl:apply-templates select="node() | @*"/>
        </xsl:element>
    </xsl:template>

    <xsl:template match="@*">
        <xsl:copy/>
    </xsl:template>
    
</xsl:stylesheet>

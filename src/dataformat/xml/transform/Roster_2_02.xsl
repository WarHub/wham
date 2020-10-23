<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" 
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
                xmlns:bsr="http://www.battlescribe.net/schema/rosterSchema" 
                xmlns="http://www.battlescribe.net/schema/rosterSchema"
                exclude-result-prefixes="bsr">

    <xsl:output method="xml" indent="yes"/>

    
    <!-- Roster -->
    <xsl:template match="/bsr:roster">
        <roster>
            <!-- Attributes -->
            <xsl:attribute name="battleScribeVersion">2.02</xsl:attribute>
            <xsl:apply-templates select="@*[name(.) != 'battleScribeVersion']"/>
            
            
            <!-- Nodes -->
            <xsl:apply-templates select="node()"/>
        </roster>
    </xsl:template>
    
    
    <!-- Cost -->
    <xsl:template match="bsr:cost">
        <cost>
            <!-- Attributes -->
            <xsl:attribute name="typeId"><xsl:value-of select="@costTypeId"/></xsl:attribute>
            <xsl:apply-templates select="@*[name(.) != 'costTypeId']"/>
        </cost>
    </xsl:template>
    
    
    <!-- CostLimit -->
    <xsl:template match="bsr:costLimit">
        <costLimit>
            <!-- Attributes -->
            <xsl:attribute name="typeId"><xsl:value-of select="@costTypeId"/></xsl:attribute>
            <xsl:apply-templates select="@*[name(.) != 'costTypeId']"/>
        </costLimit>
    </xsl:template>
    
    
    <!-- Profile -->
    <xsl:template match="bsr:profile">
        <profile>
            <!-- Attributes -->
            <xsl:attribute name="typeId"><xsl:value-of select="@profileTypeId"/></xsl:attribute>
            <xsl:attribute name="typeName"><xsl:value-of select="@profileTypeName"/></xsl:attribute>
            <xsl:apply-templates select="@*[name(.) != 'profileTypeId' and name(.) != 'profileTypeName']"/>
            
            
            <!-- Nodes -->
            <xsl:apply-templates select="node()"/>
        </profile>
    </xsl:template>
    
    
    <!-- Characteristic -->
    <xsl:template match="bsr:characteristic">
        <characteristic>
            <!-- Attributes -->
            <xsl:attribute name="typeId"><xsl:value-of select="@characteristicTypeId"/></xsl:attribute>
            <xsl:apply-templates select="@*[name(.) != 'characteristicTypeId' and name(.) != 'value']"/>
            
            
            <!-- Value -->
            <xsl:value-of select="@value"/>
        </characteristic>
    </xsl:template>
    
    
    <xsl:template match="* | bsr:*">
        <xsl:element name="{local-name(.)}">
            <xsl:apply-templates select="node() | @*"/>
        </xsl:element>
    </xsl:template>

    <xsl:template match="@*">
        <xsl:copy/>
    </xsl:template>
    
</xsl:stylesheet>
